using System;
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
    public class AgentService : IAgentService
    {
        private readonly IPersistService _persistService;

        public AgentService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<ResponseBase> GetPlayers(long operatorId, long agentId, Guid traceId, List<string> messages)
        {
            var getPlayersResult = new GetPlayersResponse();

            var players =
                await _persistService.FindMany<User>(_ => _.OperatorId == operatorId && _.AgentId == agentId);

            if (players == null || !players.Any())
            {
                getPlayersResult =
                    GetPlayersResponse.Create(false, traceId, HttpStatusCode.Forbidden,  GetPlayersResult.NoPlayerFound);
                
                return getPlayersResult;
            }

            if (!players.Any())
            {
                return getPlayersResult;
            }

            getPlayersResult =
                GetPlayersResponse.Create(true, traceId, HttpStatusCode.OK, GetPlayersResult.PlayersFound);
                
            getPlayersResult.Data = players.OrderBy(_ => _.UserName).ToList();

            return getPlayersResult;
        }
    }
}
