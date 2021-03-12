﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseBase> AddUser(AddUserRequest request, Guid traceId, List<string> messages);
    }
}