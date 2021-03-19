﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization.Interfaces
{
    public interface ICognitoUserStore
    {
        Task<Results> CreateAsync(string email, string password, List<string> messages);

        Task<GenericResponse> AuthenticateAsync(string email, string password, List<string> messages);

        Task<GenericResponse> ChangePasswordAsync(string email, string password, List<string> messages);
    }
}