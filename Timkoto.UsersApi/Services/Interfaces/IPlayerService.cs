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

        Task<GenericResponse> GetTeamsInContest(long userId, long contestId, List<string> messages);

        Task<GenericResponse> GetTeamPlayerStats(long playerTeamId, List<string> messages);

        Task<GenericResponse> GetTeamsHistory(long userId, List<string> messages);

        Task<GenericResponse> GetHomePageData(long operatorId, long userId, List<string> messages);
    }
}