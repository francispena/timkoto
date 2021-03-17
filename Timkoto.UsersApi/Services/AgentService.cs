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

        public async Task<GenericResponse> GetContestPlayers(long operatorId, long agentId, string gameDate, List<string> messages)
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
                $@"select w.contestId, w.operatorId, w.agentId, u.email, u.userName,  w.amount, w.agentCommission, prize from wager w 
                        inner join timkotodb.user u 
                        on u.id = w.userId
                        where w.contestId = {contest.Id} and w.operatorId = {operatorId} and w.agentId = {agentId};";

            var players = await _persistService.SqlQuery<PlayerPoints>(sqlQuery);

            if (players == null || !players.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoAgentFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.AgentsFound);

            var totals = players.GroupBy(q => 1)
                .Select(g => new
                {
                    TotalAmount = g.Sum(_ => _.Amount),
                    TotalAgentCommission = g.Sum(_ => _.AgentCommission),
                    TotalPrize = g.Sum(_ => _.Prize)
                })
                .Single();

            genericResponse.Data = new { Players = players, Summary = totals };

            return genericResponse;
        }
    }
}
