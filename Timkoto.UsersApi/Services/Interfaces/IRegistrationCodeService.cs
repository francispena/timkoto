using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IRegistrationCodeService
    {
        Task<GenericResponse> Generate(long userId, List<string> messages);
        
        Task<User> GenerateResetPasswordCode(string requestEmailAddress, List<string> messages);
    }
}