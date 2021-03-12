using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<ResponseBase> GetPlayer(long userId, Guid traceId, List<string> messages);
    }
}