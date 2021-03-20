using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public PlayerService(IPersistService persistService)
        {
            _persistService = persistService;
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

        public async Task<GenericResponse> GetTeams(long userId, long contestId, List<string> messages)
        {
            var genericResponse = new GenericResponse();

            var playerTeams =
                await _persistService.FindMany<PlayerTeam>(_ => _.UserId == userId && _.ContestId == contestId);

            if (playerTeams == null || !playerTeams.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoTeamFound);

                return genericResponse;
            }

            if (!playerTeams.Any())
            {
                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.TeamsFound);

            genericResponse.Data = new { PlayerTeams = playerTeams.OrderBy(_ => _.TeamName).ToList() };

            return genericResponse;
        }

        public async Task<GenericResponse> GetTeamPlayerStats(long contestId, long userId, List<string> messages)
        {
            GenericResponse genericResponse;

            var sqlQuery =
            $@"SELECT distinct concat(np.lastName, ', ', np.firstName) as playerName, nt.nickName as teamName, gp.points, gp.rebounds, 
                    gp.assists, gp.steals, gp.blocks, gp.turnOvers, gp.totalPoints FROM timkotodb.playerLineup pl
                    inner join timkotodb.gamePlayer gp
                    on gp.playerId = pl.playerId and gp.contestId = pl.contestId 
                    inner join timkotodb.nbaPlayer np
                    on np.id = gp.playerId
                    inner join timkotodb.nbaTeam nt
                    on nt.id = np.teamId
                    where pl.contestId = {contestId} and pl.userId = {userId};";

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
    }
}
