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
                $@"select pt.userId as playerId, pt.agentId, u.operatorId, u.userName, pt.teamName, pt.teamRank, pt.score, pt.prize, pt.agentCommission, pt.amount
                    from timkotodb.playerTeam pt
                    inner join timkotodb.user u
                    on u.id = pt.userId
                    where pt.operatorId = '{operatorId}' and pt.agentId = '{agentId}' and pt.contestId = '{contest.Id}'
                    order by pt.teamName;";

            var players = await _persistService.SqlQuery<PlayerPoints>(sqlQuery);

            if (players == null || !players.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoPlayerFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.PlayersFound);

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

        public async Task<GenericResponse> GetAgentPoints(long agentId, List<string> messages)
        {
            GenericResponse genericResponse;

            var sqlQuery =
                $@"select c.gameDate, sum(pt.amount) as Collectible, sum(pt.agentCommission) as Commission, sum(pt.prize) as Prize 
                    from contest c
                    inner join playerTeam pt 
                    on pt.contestId = c.id 
                    where pt.agentId = {agentId}
                    group by pt.agentId, c.gameDate
                    order by c.gameDate desc;";

            var agentPoints = await _persistService.SqlQuery<AgentPoints>(sqlQuery);

            if (agentPoints == null || !agentPoints.Any())
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.NoAgentPointsFound);

                return genericResponse;
            }

            genericResponse =
                GenericResponse.Create(true, HttpStatusCode.OK, Results.AgentPointsFound);

            genericResponse.Data = new { AgentsPoints = agentPoints };

            return genericResponse;
        }
    }
}
