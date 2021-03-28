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
    public class PlayerService : IPlayerService
    {
        private readonly IPersistService _persistService;

        private readonly ITransactionService _transactionService;

        private readonly IContestService _contestService;

        public PlayerService(IPersistService persistService, ITransactionService transactionService, IContestService contestService)
        {
            _persistService = persistService;
            _transactionService = transactionService;
            _contestService = contestService;
        }

        public async Task<GenericResponse> GetUser(long userId, List<string> messages)
        {
            GenericResponse genericResponse;

            var player =
                await _persistService.FindOne<User>(_ => _.Id == userId);

            if (player == null)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoPlayerFound);

                return genericResponse;
            }

            var playerWallet =
                await _persistService.FindLast<Transaction>(_ => _.UserId == userId, _ => _.CreateDateTime);

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayerFound);

            genericResponse.Data = new { Player = player, Wallet = playerWallet };


            return genericResponse;
        }

        public async Task<GenericResponse> GetPlayers(long operatorId, long agentId, List<string> messages)
        {
            var genericResponse = new GenericResponse();

            var players =
                await _persistService.FindMany<User>(_ => _.OperatorId == operatorId && _.AgentId == agentId);

            if (players == null || !players.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoPlayerFound);

                return genericResponse;
            }

            if (!players.Any())
            {
                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayersFound);

            genericResponse.Data = new { Players = players.OrderBy(_ => _.UserName).ToList() };

            return genericResponse;
        }

        public async Task<GenericResponse> GetTeamsInContest(long userId, long contestId, List<string> messages)
        {
            var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);

            if (contest == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoContestFound);
            }

            var playerTeams =
                await _persistService.FindMany<PlayerTeam>(_ => _.UserId == userId && _.ContestId == contest.Id);

            if (playerTeams == null || !playerTeams.Any())
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoTeamFound);
            }

            var genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.TeamsFound);

            genericResponse.Data = new { PlayerTeams = playerTeams.OrderBy(_ => _.TeamName).ToList() };

            return genericResponse;
        }

        public async Task<GenericResponse> GetTeamPlayerStats(long playerTeamId, List<string> messages)
        {
            GenericResponse genericResponse;

            var sqlQuery =
            $@"SELECT concat(np.lastName, ', ', np.firstName) as playerName, nt.nickName as teamName, gp.points, gp.rebounds, 
                gp.assists, gp.steals, gp.blocks, gp.turnOvers, gp.totalPoints FROM timkotodb.playerLineup pl
                inner join timkotodb.gamePlayer gp
                on gp.playerId = pl.playerId and gp.contestId = pl.contestId 
                inner join timkotodb.nbaPlayer np
                on np.id = gp.playerId
                inner join timkotodb.nbaTeam nt
                on nt.id = np.teamId
                inner join timkotodb.playerTeam pt 
                on pt.id = pl.playerTeamId and pt.contestId = pl.contestId
                where pt.id = '{playerTeamId}'";

            var teamPlayerStats = await _persistService.SqlQuery<PlayerStats>(sqlQuery);

            if (teamPlayerStats == null || !teamPlayerStats.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoTeamPlayersFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.TeamPlayersFound);

            genericResponse.Data = new { PlayerStats = teamPlayerStats };

            return genericResponse;
        }

        public async Task<GenericResponse> GetTeamsHistory(long userId, List<string> messages)
        {
            var genericResponse = new GenericResponse();

            var sqlQuery =
                $@"SELECT pt.Id, pt.contestId, c.gameDate, pt.teamName, pt.score, pt.teamRank, pt.prize 
                    FROM timkotodb.playerTeam pt
                    inner join contest c
                    on c.Id = pt.contestId
                    where userId = '{userId}';";

            var playerTeamHistory = await _persistService.SqlQuery<PlayerTeamHistory>(sqlQuery);

            if (playerTeamHistory == null || !playerTeamHistory.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoTeamFound);

                return genericResponse;
            }

            if (!playerTeamHistory.Any())
            {
                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.TeamsFound);

            genericResponse.Data = new { PlayerTeams = playerTeamHistory.OrderByDescending(_ => _.GameDate).ToList() };

            return genericResponse;
        }

        public async Task<GenericResponse> GetHomePageData(long operatorId, long userId, List<string> messages)
        {
            var user = _persistService.FindOne<User>(_ =>
                _.IsActive && _.UserType == UserType.Player && _.Id == userId);
            if (user == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.PlayerNotFound);
            }

            var genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayerFound);

            var balance = await _transactionService.Balance(userId, messages);

            var prizePool = await _contestService.PrizePool(operatorId, messages);

            if (prizePool?.Data?.PrizePool == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.PrizePoolNotSet);
            }

            var contest = await _persistService.FindOne<Contest>(_ =>
                _.ContestState != ContestState.Finished);
            
            if (contest == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoContestFound);
            }

            var teams = await _contestService.GetGames(contest.Id, messages);

            var sqlQuery =
                $@"SELECT totalPackage, entryPoints, tag FROM timkotodb.contestPrize cpr
                        inner join timkotodb.contestPool cpl 
                        on cpl.contestPrizeId = cpr.id
                        where cpl.operatorId =  '{operatorId}' and cpl.contestId = '{contest.Id}';";

            var contestPackages = await _persistService.SqlQuery<ContestPackage>(sqlQuery);

            var contestPackage = contestPackages?.FirstOrDefault();
            if (contestPackage == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoContestPackage);
            }

            genericResponse.Data  = new 
            {
                balance.Data.Balance,
                prizePool.Data.PrizePool,
                contest,
                teams.Data.Teams,
                contestPackage
            };

            return genericResponse;
        }
    }
}
