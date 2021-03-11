using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
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

        public async Task<ResponseBase> AddUser(User user, string code, Guid traceId, List<string> messages)
        {
            var addUserResponse = new AddUserResponse();

            var registrationCode = await _persistService.FindOne<RegistrationCode>(_ => _.Code == code && _.IsActive);
            if (registrationCode == null)
            {
                addUserResponse =
                    AddUserResponse.Create(false, traceId, HttpStatusCode.Forbidden, AddNewUserResult.InvalidRegistrationCode);
                
                return addUserResponse;
            }
            if (DateTime.UtcNow.Subtract(registrationCode.CreateDateTime).TotalMinutes > 120)
            {
                addUserResponse =
                    AddUserResponse.Create(false, traceId, HttpStatusCode.Forbidden, AddNewUserResult.InvalidRegistrationCode);

                return addUserResponse;
            }

            user.OperatorId = registrationCode.OperatorId;
            user.AgentId = registrationCode.AgentId;
            user.UserType = registrationCode.UserType;

            var result = await _persistService.Save(user);
            
            registrationCode.IsActive = false;
            await _persistService.Update(registrationCode);

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
