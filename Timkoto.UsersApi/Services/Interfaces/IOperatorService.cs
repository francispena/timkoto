using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IOperatorService
    {
        Task<GenericResponse> GetAgents(long operatorId, List<string> messages);

        Task<GenericResponse> GetContestAgents(long operatorId, string gameDate, List<string> messages);

        Task<GenericResponse> GetContestPlayers(long operatorId, string gameDate, List<string> messages);

        Task<List<ContestAgentPointsForDownload>> GetContestAgentsForDownload(long operatorId, string gameDate, List<string> messages);
    }
}