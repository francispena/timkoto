using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class ContestService : IContestService
    {
        private readonly IPersistService _persistService;

        public ContestService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<GenericResponse> GetGames(string gameDate, List<string> messages)
        {
            GenericResponse genericResponse;

            var contest = await _persistService.FindOne<Contest>(_ => _.GameDate == gameDate);
            if (contest == null)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.GameNotFound);

                return genericResponse;
            }

            var sqlQuery =
                $@"SELECT  ht.fullName as homeTeamName, ht.nickname as homeTeamNickName, vt.fullName as visitorTeamName, vt.nickname as visitorTeamNickName, 
                    date_format(convert_tz(g.startTime , '+00:00', '+08:00'),'%h:%i') as startTime, ht.logo as homeTeamLogo, vt.logo as visitorTeamLogo FROM timkotodb.game g 
                    inner join nbaTeam ht
                    on ht.id = g.hteamid
                    inner join nbaTeam vt
                    on vt.id = g.vteamid
                    where g.contestId = '{contest.Id}' order by g.startTime;";

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

        public async Task<GenericResponse> GetPlayers(string gameDate, List<string> messages)
        {
            GenericResponse genericResponse;

            var contest = await _persistService.FindOne<Contest>(_ => _.GameDate == gameDate);
            if (contest == null)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.GameNotFound);

                return genericResponse;
            }

            var sqlQuery =
                $@"SELECT np.id as playerId, concat(lastName, ', ', firstName) as playerName, np.jersey, nt.nickName as team, np.position, gp.salary FROM timkotodb.gamePlayer gp 
                        inner join nbaPlayer np
                        on gp.playerId = np.id
                        inner join nbaTeam nt 
                        on nt.id = np.teamId 
                        where gp.contestId = '{contest.Id}';";

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

            if (request.LineUpTeam.UserId== 0)
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
                Amount = 100,
                AgentCommission = (decimal)(100 * 0.05)
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
                var groupedTeamPoints = teamPoints.GroupBy(_ => _.OperatorId)
                    .Select(g => new { OperatorId = g.Key, TeamsToRank = g.ToList() }).ToList();

                var teamPointsToUpdate = new List<TeamPoints>();
                
                //process by operatorId
                foreach (var groupedTeamPoint in groupedTeamPoints)
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

                    var prizePool = await _persistService.FindMany<PrizePool>(_ =>
                        _.OperatorId == groupedTeamPoint.OperatorId && _.ContestId == contest.Id);
                    
                    if (prizePool == null || !prizePool.Any())
                    {
                        prizePool = await _persistService.FindMany<PrizePool>(_ =>
                            _.OperatorId == groupedTeamPoint.OperatorId && _.ContestId == 0);
                    }

                    if (prizePool == null || !prizePool.Any())
                    {
                        return false;
                    }

                    var prizeQueue = new Queue();

                    foreach (var prize in prizePool)
                    {
                        for (var i = prize.FromRank; i <= prize.ToRank; i++)
                        {
                            prizeQueue.Enqueue(prize.Prize);
                        }
                    }

                    //group by rank
                    var groupedTeamRank = sortedTeamPoints.GroupBy(_ => _.TeamRank)
                        .Select(g => new { TeamRank = g.Key, RanksToPrize = g.ToList() }).ToList();

                    //loop through each rank group
                    foreach (var teamRank in groupedTeamRank.OrderBy(_ => _.TeamRank).ToList())
                    {
                        var rankPrize = 0m;
                        for (var i = 0; i < teamRank.RanksToPrize.Count; i++)
                        {
                            rankPrize +=  Convert.ToDecimal(prizeQueue.Dequeue());
                            
                            if (prizeQueue.Count == 0)
                            {
                                break;
                            }
                        }

                        foreach (var team in teamRank.RanksToPrize)
                        {
                            team.Prize = rankPrize / teamRank.RanksToPrize.Count;
                        }

                        if (prizeQueue.Count == 0)
                        {
                            break;
                        }
                    }

                    teamPointsToUpdate.AddRange(groupedTeamRank.SelectMany(_ => _.RanksToPrize).ToList());
                }

                var sqlUpdate = string.Join(";", teamPointsToUpdate.Select(_ => $"UPDATE `timkotodb`.`playerTeam` SET `score` = '{_.TotalPoints}', `teamRank` = '{_.TeamRank}', `prize` = '{_.Prize}' WHERE (`id` = '{_.PlayerTeamId}')"));

                var updateResult = await _persistService.ExecuteSql(sqlUpdate +";");

                return updateResult;
            }
            catch (Exception ex)
            {

                return true;
            }

            return false;
        }
    }
}
