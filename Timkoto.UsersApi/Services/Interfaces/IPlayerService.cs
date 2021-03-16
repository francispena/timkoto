using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<GenericResponse> GetPlayer(long userId, List<string> messages);

        Task<GenericResponse> GetPlayers(long operatorId, long agentId, List<string> messages);
    }
}