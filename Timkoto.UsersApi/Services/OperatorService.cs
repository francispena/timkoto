using System;
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
    public class OperatorService : IOperatorService
    {
        private readonly IPersistService _persistService;

        public OperatorService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<GenericResponse> GetAgents(long operatorId, List<string> messages)
        {
            GenericResponse getAgentsResult;

            var agents =
                await _persistService.FindMany<User>(_ =>
                    _.IsActive && _.OperatorId == operatorId && _.UserType == UserType.Agent);

            if (agents == null || !agents.Any())
            {
                getAgentsResult =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoAgentFound);

                return getAgentsResult;
            }

            getAgentsResult =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.AgentsFound);

            getAgentsResult.Data = new { Agents = agents };

            return getAgentsResult;
        }
    }
}
