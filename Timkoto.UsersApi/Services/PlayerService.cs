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

        public async Task<GenericResponse> GetPlayer(long userId, List<string> messages)
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

    }
}
