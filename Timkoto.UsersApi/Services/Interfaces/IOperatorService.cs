using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IOperatorService
    {
        Task<GenericResponse> GetAgents(long operatorId, List<string> messages);
    }
}