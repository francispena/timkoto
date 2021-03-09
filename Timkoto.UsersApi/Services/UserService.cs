using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class UserService : IUserService
    {
        private readonly IPersistService _persistService;

        public UserService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<ResponseBase> AddUser(User user, Guid traceId, List<string> messages)
        {
            var result = await _persistService.Save(user);
            
            var addUserResponse = new AddUserResponse();

            if (result > 0)
            {
                addUserResponse =
                    AddUserResponse.Create(true, traceId, HttpStatusCode.OK, AddNewUserResult.NewUserCreated);
            }
            else if (result == -1000)
            {
                addUserResponse =
                    AddUserResponse.Create(false, traceId, HttpStatusCode.Forbidden, AddNewUserResult.EmailAddressExists);
            }

            return addUserResponse;
        }
    }
}
