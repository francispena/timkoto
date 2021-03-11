﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseBase> AddUser(User user, string code, Guid traceId, List<string> messages);
    }
}