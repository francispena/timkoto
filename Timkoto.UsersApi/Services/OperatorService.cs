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
            GenericResponse genericResponse;

            var agents =
                await _persistService.FindMany<User>(_ =>
                    _.IsActive && _.OperatorId == operatorId && _.UserType == UserType.Agent);

            if (agents == null || !agents.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoAgentFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.AgentsFound);

            genericResponse.Data = new { Agents = agents };

            return genericResponse;
        }

        public async Task<GenericResponse> GetContestAgents(long operatorId, string gameDate,  List<string> messages)
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
                $@"select w.ContestId, w.OperatorId, u.Email, u.UserName, w.AgentId, sum(w.amount) as Collectible, sum(w.agentCommission) as Commission,  sum(w.prize) as Prize 
                    from wager w 
                    inner join timkotodb.user u
                    on u.id = w.agentId
                    where w.contestId = {contest.Id} and w.operatorId = {operatorId}
                    group by w.agentId, u.email, u.userName, w.contestId, w.operatorId;";

            var agents = await _persistService.SqlQuery<AgentPoints>(sqlQuery);

            if (agents == null || !agents.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoAgentFound);

                return genericResponse;
            }

            var totals = agents.GroupBy(q => 1)
                .Select(g => new
                {
                    TotalCollectible = g.Sum(_ => _.Collectible),
                    TotalAgentCommission = g.Sum(_ => _.Commission),
                    TotalPrize = g.Sum(_ => _.Prize)
                })
                .Single();

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.AgentsFound);

            genericResponse.Data = new { Agents = agents, Summary = totals };

            return genericResponse;
        }
    }
}
