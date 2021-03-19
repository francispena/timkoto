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
                $@"select pt.ContestId, pt.OperatorId, u.Email, u.UserName, pt.AgentId, sum(pt.amount) as Collectible, 
                    sum(pt.agentCommission) as Commission, sum(pt.prize) as Prize 
	                    from 
	                    playerTeam pt 
                        inner join timkotodb.user u
	                    on pt.agentId = u.id
	                    where pt.contestId = {contest.Id} and pt.operatorId = {operatorId}
	                    group by pt.agentId, u.email, u.userName, pt.contestId, pt.operatorId 
	                    order by u.userName;";

            var agents = await _persistService.SqlQuery<ContestAgentPoints>(sqlQuery);

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
