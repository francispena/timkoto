using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization.Interfaces
{
    public interface ICognitoUserStore
    {
        Task<Results> CreateAsync(string userName, string password, List<string> messages);

        Task<GenericResponse> AuthenticateAsync(string userName, string password, List<string> messages);

        Task<GenericResponse> ChangePasswordAsync(string userName, string password, List<string> messages);
    }
}