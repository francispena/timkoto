﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data;
using Newtonsoft.Json;
using NHibernate;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;
using Ubiety.Dns.Core.Records;

namespace Timkoto.UsersApi.Services
{
    public class ContestService : IContestService
    {
        private readonly IPersistService _persistService;

        public ContestService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<GenericResponse> GetGames(long contestId, List<string> messages)
        {
            GenericResponse genericResponse;

            var sqlQuery =
                $@"SELECT  ht.fullName as homeTeamName, ht.nickname as homeTeamNickName, vt.fullName as visitorTeamName, vt.nickname as visitorTeamNickName, 
                    date_format(convert_tz(g.startTime , '+00:00', '+08:00'),'%h:%i') as startTime, ht.logo as homeTeamLogo, vt.logo as visitorTeamLogo FROM timkotodb.game g 
                    inner join nbaTeam ht
                    on ht.id = g.hteamid
                    inner join nbaTeam vt
                    on vt.id = g.vteamid
                    where g.contestId = '{contestId}' order by g.startTime;";

            var teams = await _persistService.SqlQuery<ContestTeam>(sqlQuery);

            if (teams == null || !teams.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoContestTeamFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.ContestTeamFound);

            genericResponse.Data = new { Teams = teams };

            return genericResponse;
        }

        public async Task<GenericResponse> GetPlayers(long contestId, List<string> messages)
        {
            GenericResponse genericResponse;

            var sqlQuery =
                $@"SELECT np.id as playerId, concat(lastName, ', ', firstName) as playerName, np.jersey, nt.nickName as team, np.position, gp.salary FROM timkotodb.gamePlayer gp 
                        inner join nbaPlayer np
                        on gp.playerId = np.id
                        inner join nbaTeam nt 
                        on nt.id = np.teamId 
                        where gp.contestId = '{contestId}';";

            var players = await _persistService.SqlQuery<ContestPlayer>(sqlQuery);

            if (players == null || !players.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoPlayerFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayerFound);

            var groupedPlayers = players.GroupBy(_ => _.Position).Select(g => new { Position = g.Key, Players = g.ToList() }).ToList();

            genericResponse.Data = groupedPlayers;

            return genericResponse;
        }

        public async Task<GenericResponse> SubmitLineUp(LineUpRequest request, List<string> messages)
        {
            var selectedCount = request.LineUp.SelectMany(_ => _.Players).Count(_ => _.Selected);
            if (selectedCount != 9)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidLineUpCount);
            }

            if (string.IsNullOrWhiteSpace(request.LineUpTeam.TeamName))
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.TeamNameMissing);
            }

            if (request.LineUpTeam.OperatorId == 0)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidOperatorId);
            }

            if (request.LineUpTeam.AgentId == 0)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidAgentID);
            }

            if (request.LineUpTeam.UserId == 0)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidUserId);
            }

            if (request.LineUpTeam.ContestId == 0)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidContestId);
            }

            var contest = await _persistService.FindOne<Contest>(_ => _.Id == request.LineUpTeam.ContestId);

            if (contest == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoContestFound);
            }
            if (contest.ContestState != ContestState.Upcoming)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.TeamSubmissionNotAccepted);
            }

            var isUpdate = request.LineUpTeam.PlayerTeamId > 0;

            var hash = string.Join("-",
                request.LineUp.SelectMany(_ => _.Players).Where(_ => _.Selected).Select(_ => _.PlayerId)
                    .OrderBy(_ => _));

            var playerTeam = new PlayerTeam
            {
                OperatorId = request.LineUpTeam.OperatorId,
                AgentId = request.LineUpTeam.AgentId,
                ContestId = request.LineUpTeam.ContestId,
                UserId = request.LineUpTeam.UserId,
                TeamName = request.LineUpTeam.TeamName,
                LineupHash = hash,
                Amount = contest.EntryPoints,
                AgentCommission = (contest.EntryPoints * 0.05m)
            };

            var dbSession = _persistService.GetSession();
            var tx = dbSession.BeginTransaction();

            try
            {
                if (isUpdate)
                {
                    var existingPlayerTeam =
                        await _persistService.FindOne<PlayerTeam>(_ => _.Id == request.LineUpTeam.PlayerTeamId && _.UserId == request.LineUpTeam.UserId);

                    if (existingPlayerTeam == null)
                    {
                        return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoLineUpToUpdate);
                    }

                    await dbSession.DeleteAsync(existingPlayerTeam);

                    await dbSession
                        .CreateSQLQuery(
                            "DELETE FROM `timkotodb`.`playerLineup` WHERE (`playerTeamId` = :playerTeamId);")
                        .SetParameter("playerTeamId", request.LineUpTeam.PlayerTeamId)
                        .ExecuteUpdateAsync();
                }

                var saveTeamResult = await dbSession.SaveAsync(playerTeam);

                if ((long)saveTeamResult > 0)
                {
                    var sqlInsert =
                        "INSERT INTO `timkotodb`.`playerLineup` (`operatorId`, `contestId`, `userId`, `playerTeamId`, `playerId`) VALUES ";
                    var sqlValues =
                        string.Join(",",
                            request.LineUp.SelectMany(_ => _.Players).Where(_ => _.Selected).Select(_ =>
                                $"({request.LineUpTeam.OperatorId}, {request.LineUpTeam.ContestId}, {request.LineUpTeam.UserId}, {playerTeam.Id}, {_.PlayerId})"));

                    await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues};").ExecuteUpdateAsync();
                }

                if (!isUpdate)
                {
                    var lastTransaction = await
                        _persistService.FindLast<Transaction>(_ => _.UserId == request.LineUpTeam.UserId,
                            _ => _.CreateDateTime);

                    if (lastTransaction == null)
                    {
                        await tx.RollbackAsync();
                        return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NotEnoughPoints);
                    }

                    if (lastTransaction.Balance < contest.EntryPoints)
                    {
                        await tx.RollbackAsync();
                        return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NotEnoughPoints);
                    }

                    var newTransaction = new Transaction
                    {
                        Amount = -contest.EntryPoints,
                        OperatorId = request.LineUpTeam.OperatorId,
                        UserType = UserType.Player,
                        TransactionType = TransactionType.WalletCredit,
                        UserId = request.LineUpTeam.UserId,
                        Balance = lastTransaction.Balance + -contest.EntryPoints
                    };

                    var saveTransactionResult = await dbSession.SaveAsync(newTransaction);
                    if ((long)saveTransactionResult <= 0)
                    {
                        await tx.RollbackAsync();

                        return GenericResponse.Create(false, HttpStatusCode.Forbidden,
                            Results.ProcessingTransactionFailed);
                    }
                }

                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();

                return GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayerLineUpCreated);
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }
            }
            finally
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                if (dbSession.IsOpen)
                {
                    dbSession.Close();
                    dbSession.Dispose();
                }
            }

            return default;
        }

        public async Task<string> SetPrizes(List<string> messages)
        {
            ITransaction tx = null;
            var result = "";
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return "No Ongoing contest";
                }

                var sqlQuery =
                    $@"select operatorId, userId, agentId, id as playerTeamId, teamRank from timkotodb.playerTeam where contestId = {contest.Id} and score > 0;";

                var teamPoints = await _persistService.SqlQuery<TeamPoints>(sqlQuery);

                //group by operatorId
                var groupedTeamsByOperatorId = teamPoints.GroupBy(_ => _.OperatorId)
                    .Select(g => new { OperatorId = g.Key, TeamsToSetPrize = g.ToList() }).ToList();

                var teamPointsToUpdate = new List<TeamPoints>();
                var actualPrizePools = new List<ActualPrizePool>();

                //process by operatorId
                foreach (var groupedTeamPoint in groupedTeamsByOperatorId)
                {
                    var contestPool =
                         await _persistService.FindOne<ContestPool>(_ =>
                             _.ContestId == contest.Id && _.OperatorId == groupedTeamPoint.OperatorId);

                    if (contestPool == null)
                    {
                        continue;
                    }

                    var prizePool = await _persistService.FindMany<PrizePool>(_ => _.ContestPrizeId == contestPool.ContestPrizeId);

                    if (prizePool == null || !prizePool.Any())
                    {
                        continue;
                    }
                    
                    var prizeQueue = new Queue();

                    foreach (var prize in prizePool)
                    {
                        actualPrizePools.Add(new ActualPrizePool
                        {
                            ContestId = contest.Id,
                            OperatorId = groupedTeamPoint.OperatorId,
                            FromRank = prize.FromRank,
                            ToRank = prize.ToRank,
                            Prize = prize.Prize
                        });

                        for (var i = prize.FromRank; i <= prize.ToRank; i++)
                        {
                            prizeQueue.Enqueue(prize.Prize);
                        }
                    }

                    var packageTotal = prizeQueue.ToArray().Sum(_ => (decimal)_);

                    //group by rank
                    var groupedTeamRank = groupedTeamPoint.TeamsToSetPrize.GroupBy(_ => _.TeamRank)
                        .Select(g => new { TeamRank = g.Key, RanksToPrize = g.ToList() }).ToList();

                    //loop through each rank group
                    foreach (var teamRank in groupedTeamRank.OrderBy(_ => _.TeamRank).ToList())
                    {
                        var rankPrize = 0m;
                        foreach (var team in teamRank.RanksToPrize)
                        {
                            rankPrize += Convert.ToDecimal(prizeQueue.Dequeue());

                            if (prizeQueue.Count == 0)
                            {
                                break;
                            }
                        }

                        foreach (var team in teamRank.RanksToPrize)
                        {
                            team.Prize = decimal.Round(rankPrize / teamRank.RanksToPrize.Count, MidpointRounding.ToZero);
                        }

                        if (prizeQueue.Count == 0)
                        {
                            break;
                        }
                    }

                    teamPointsToUpdate.AddRange(groupedTeamRank.SelectMany(_ => _.RanksToPrize).ToList());
                    
                    result =
                        $"{result}{Environment.NewLine}ContesId:{contest.Id}, OperatorId:{groupedTeamPoint.OperatorId}, PackageTotal:{packageTotal} TotaPrize: {teamPointsToUpdate.Sum(_ => _.Prize)}";
                }

                //insert actualPrizePool
                var sqlInsert =
                    "INSERT INTO `timkotodb`.`actualPrizePool` (`contestId`, `operatorId`, `fromRank`, `toRank`, `prize`) VALUES ";
                var sqlValues = string.Join(",", actualPrizePools.Select(_ => $"('{contest.Id}','{_.OperatorId}', '{_.FromRank}', '{_.ToRank}', '{_.Prize}')"));

                //update prize to zero in playerTeam table
                var sqlUpdateToZero = $"UPDATE `timkotodb`.`playerTeam` SET `prize` = '0' WHERE (`contestId` = '{contest.Id}');";

                //update prize in playerTeam table
                var sqlUpdate = string.Join(" ", teamPointsToUpdate.Select(_ => $"UPDATE `timkotodb`.`playerTeam` SET `prize` = '{_.Prize}' WHERE (`id` = '{_.PlayerTeamId}');"));

                var dbSession = _persistService.GetSession();
                tx = dbSession.BeginTransaction();

                await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues}").ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdateToZero).ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdate).ExecuteUpdateAsync();
                
                await tx.CommitAsync();

                return result;
            }
            catch (Exception ex)
            {

                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }
                return ex.Message;
            }
        }

        public async Task<string> SetPrizesInTransaction(List<string> messages)
        {
            ITransaction tx = null;

            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return "No contest";
                }

                if (contest.ContestState == ContestState.Finished)
                {
                    return "Contest is marked as finished.";
                }

                var sqlQuery =
                    $@"select pt.operatorId, pt.userId, pt.agentId, sum(pt.prize) as prize, u.points from timkotodb.playerTeam pt
                        inner join timkotodb.user u 
                        on u.id = pt.userId
                        where pt.contestId = {contest.Id} and pt.score > 0 and pt.prize > 0
                        group by pt.operatorId, pt.userId, pt.agentId, u.points;";

                var teamPoints = await _persistService.SqlQuery<TeamPoints>(sqlQuery);

                if (teamPoints == null || !teamPoints.Any())
                {
                    return "No Team Points";
                }

                //update balance in user table
                var sqlUpdate = string.Join(" ", teamPoints.Select(_ => $"UPDATE `timkotodb`.`user` SET `points` = '{_.Points + _.Prize}' WHERE (`id` = '{_.UserId}');"));
                
                //insert into transaction
                var sqlInsert =
                    "INSERT INTO `timkotodb`.`transaction` (`operatorId`, `agentId`, `userId`, `userType`, `transactionType`, `amount`, `balance`, `tag`) VALUES ";
                var sqlValues =
                    string.Join(",",
                        teamPoints.Select(_ => $"('{_.OperatorId}', '{_.AgentId}', '{_.UserId}', 'Player', 'WalletDebit', '{_.Prize}', '{_.Points + _.Prize}', '{contest.GameDate} - Prize Won')"));
                
                var dbSession = _persistService.GetSession();
                tx = dbSession.BeginTransaction();

                await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues}").ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdate).ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery($"UPDATE `timkotodb`.`contest` SET `contestState` = 'Finished' WHERE(`id` = '{contest.Id}');").ExecuteUpdateAsync();
                
                await tx.CommitAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                if (tx != null &&  tx.IsActive)
                {
                    await tx.RollbackAsync();
                }
                return ex.Message;
            }
        }

        public async Task<bool> RankTeams(List<string> messages)
        {
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return false;
                }

                var sqlQuery =
                    $@"select pl.operatorId, pl.playerTeamId, sum(gp.totalPoints) totalPoints
                    from timkotodb.playerLineup pl
                    inner join timkotodb.gamePlayer gp
                    on gp.contestId = pl.contestId and gp.playerId = pl.playerId
                    where pl.contestId = '{contest.Id}'
                    group by pl.operatorId, pl.playerTeamId;";

                var teamPoints = await _persistService.SqlQuery<TeamPoints>(sqlQuery);

                //group by operatorId
                var teamsByOperatorId = teamPoints.GroupBy(_ => _.OperatorId)
                    .Select(g => new { OperatorId = g.Key, TeamsToRank = g.ToList() }).ToList();

                var teamsToUpdate = new List<TeamPoints>();

                //process by operatorId
                foreach (var groupedTeamPoint in teamsByOperatorId)
                {
                    var teamsToRank = groupedTeamPoint.TeamsToRank;

                    var sortedTeamPoints = teamsToRank.OrderByDescending(_ => _.TotalPoints).ToArray();
                    //rank
                    sortedTeamPoints[0].TeamRank = 1;
                    for (var i = 1; i < sortedTeamPoints.Length; i++)
                    {
                        sortedTeamPoints[i].TeamRank = sortedTeamPoints[i].TotalPoints == sortedTeamPoints[i - 1].TotalPoints
                            ? sortedTeamPoints[i - 1].TeamRank
                            : i + 1;
                    }

                    teamsToUpdate.AddRange(sortedTeamPoints);
                }

                var sqlUpdate = string.Join(";", teamsToUpdate.Select(_ => $"UPDATE `timkotodb`.`playerTeam` SET `score` = '{_.TotalPoints}', `teamRank` = '{_.TeamRank}', `prize` = '0' WHERE (`id` = '{_.PlayerTeamId}')"));

                var updateResult = await _persistService.ExecuteSql(sqlUpdate + ";");

                return updateResult;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<GenericResponse> PrizePool(long operatorId, List<string> messages)
        {
            var sqlQuery =
                $@"SELECT pp.id, fromRank, toRank, prize FROM timkotodb.contest c 
                inner join timkotodb.contestPool cp
                on cp.contestId = c.id
                inner join timkotodb.prizePool pp
                on pp.contestPrizeId = cp.contestPrizeId
                where c.contestState in ('Upcoming', 'Ongoing') and '{operatorId}';";

            var contestPrizePool = await _persistService.SqlQuery<ContestPrizePool>(sqlQuery);

            if (contestPrizePool == null || !contestPrizePool.Any())
            {
                return GenericResponse.Create(false, HttpStatusCode.OK, Results.PrizePoolNotSet);
            }

            await ComputePrizePool(operatorId, contestPrizePool);

            var genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.PrizePoolFound);
            genericResponse.Data = new
            {
                PrizePool = contestPrizePool.OrderBy(_ => _.FromRank)
            };

            return genericResponse;
        }

        public async Task ComputePrizePool(long operatorId, List<ContestPrizePool> contestPrizePool)
        {
            string sqlQuery;
            sqlQuery =
                $@"SELECT sum(amount) as points FROM timkotodb.playerTeam where contestId = 1 && operatorId = '{operatorId}';";

            const decimal operationsCost = 1000m;
            const decimal agentCommssionPercent = 0.05m;
            var potPoints = await _persistService.SqlQuery<PotPoints>(sqlQuery);

            if (potPoints != null && potPoints.Any())
            {
                var grossPoints = potPoints[0].Points;
                var packagePoints = 0m;

                foreach (var prize in contestPrizePool)
                {
                    for (var i = prize.FromRank; i <= prize.ToRank; i++)
                    {
                        packagePoints += prize.Prize;
                    }
                }

                var expenses = (grossPoints * agentCommssionPercent) + packagePoints + operationsCost;

                var netPoints = grossPoints - expenses;

                if (netPoints > 1000)
                {
                    var addPoints = netPoints * 0.1m;
                    foreach (var prize in contestPrizePool)
                    {
                        prize.Prize += decimal.Round((prize.Prize / packagePoints) * addPoints, MidpointRounding.ToZero);
                    }
                }
            }

            foreach (var prizePool in contestPrizePool)
            {
                prizePool.DisplayRank = prizePool.FromRank == prizePool.ToRank
                    ? prizePool.FromRank.ToString()
                    : $"{prizePool.FromRank} - {prizePool.ToRank}";
            }
        }

        public async Task<GenericResponse> TeamRanks(long operatorId, List<string> messages)
        {
            var sqlQuery =
                $@"select userName, teamName, score, teamRank, prize from timkotodb.contest c
                        inner join timkotodb.playerTeam pt
                        on pt.contestId = c.id
                        inner join timkotodb.user u
                        on u.id = pt.userId
                        where c.contestState = 'Ongoing' and pt.operatorId = '{operatorId}' and teamRank > '0';";

            var teamRankPrizes = await _persistService.SqlQuery<TeamRankPrize>(sqlQuery);

            if (teamRankPrizes == null || !teamRankPrizes.Any())
            {
                return GenericResponse.Create(false, HttpStatusCode.OK, Results.NoContestTeamFound);
            }

            var genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.ContestTeamFound);
            genericResponse.Data = new
            {
                TeamRankPrizes = teamRankPrizes.OrderBy(_ => _.TeamRank)
            };

            return genericResponse;
        }

        public async Task<bool> BroadcastRanks(List<string> messages)
        {
            try
            {
                var sqlQuery =
                    $@"select pt.operatorId, userId, userName, teamName, score, teamRank, prize from timkotodb.contest c
                        inner join timkotodb.playerTeam pt
                        on pt.contestId = c.id
                        inner join timkotodb.user u
                        on u.id = pt.userId
                        where c.contestState = 'Ongoing' and teamRank > 0;";

                var teamRankPrizes = await _persistService.SqlQuery<TeamRankPrize>(sqlQuery);

                if (teamRankPrizes == null || !teamRankPrizes.Any())
                {
                    return false;
                }

                var groupedTeamRankPrizes = teamRankPrizes.GroupBy(_ => 0)
                    .Select(g => new { OperatorId = g.Key, OperatorTeams = g.ToList() }).ToList();

                foreach (var groupedTeamRankPrize in groupedTeamRankPrizes)
                {
                    if (groupedTeamRankPrize.OperatorTeams == null || !groupedTeamRankPrize.OperatorTeams.Any())
                    {
                        continue;
                    }

                    var cws = new ClientWebSocket();

                    var cancelSource = new CancellationTokenSource();
                    var connectionUri = new Uri($"wss://4a4vv008xj.execute-api.ap-southeast-1.amazonaws.com/Dev?operatorId={groupedTeamRankPrize.OperatorId}");
                    //var connectionUri = new Uri($"wss://4a4vv008xj.execute-api.ap-southeast-1.amazonaws.com/Dev");
                    await cws.ConnectAsync(connectionUri, cancelSource.Token);

                    var dataToSend = new WsData
                    {
                        message = "sendmessage",
                        data = JsonConvert.SerializeObject(groupedTeamRankPrize.OperatorTeams)
                    };

                    //var message = new ArraySegment<byte>(
                    //    Encoding.UTF8.GetBytes(
                    //        "{\"message\":\"sendmessage\", \"data\":\"Hello from .NET ClientWebSocket\"}"));

                    var message = new ArraySegment<byte>(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSend)));

                    await cws.SendAsync(message, WebSocketMessageType.Text, true, cancelSource.Token);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<GenericResponse> TeamHistoryRanks(long operatorId, string gameDate, List<string> messages)
        {
            var contest = await _persistService.FindOne<Contest>(_ => _.GameDate == gameDate);
            if (contest == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.GameNotFound);
            }

            var sqlQuery =
                $@"select userName, teamName, score, teamRank, prize from timkotodb.contest c
                        inner join timkotodb.playerTeam pt
                        on pt.contestId = c.id
                        inner join timkotodb.user u
                        on u.id = pt.userId
                        where c.id = '{contest.Id}' and pt.operatorId = '{operatorId}' and teamRank > '0';";

            var teamRankPrizes = await _persistService.SqlQuery<TeamRankPrize>(sqlQuery);

            if (teamRankPrizes == null || !teamRankPrizes.Any())
            {
                return GenericResponse.Create(false, HttpStatusCode.OK, Results.NoContestTeamFound);
            }

            var genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.ContestTeamFound);
            genericResponse.Data = new
            {
                TeamRankPrizes = teamRankPrizes.OrderBy(_ => _.TeamRank)
            };

            return genericResponse;
        }
    }
}
