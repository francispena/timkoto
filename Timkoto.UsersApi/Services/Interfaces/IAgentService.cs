using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IAgentService
    {
        Task<GenericResponse> GetContestPlayers(long operatorId, long agentId, string gameDate, List<string> messages);

        Task<GenericResponse> GetAgentPoints(long agentId, List<string> messages);
    }
}