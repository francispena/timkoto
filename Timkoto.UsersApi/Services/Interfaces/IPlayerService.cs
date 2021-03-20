using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<GenericResponse> GetUser(long userId, List<string> messages);

        Task<GenericResponse> GetPlayers(long operatorId, long agentId, List<string> messages);

        Task<GenericResponse> GetTeams(long userId, long contestId, List<string> messages);
        
        Task<GenericResponse> GetTeamPlayerStats(long contestId, long userId, List<string> messages);
    }
}