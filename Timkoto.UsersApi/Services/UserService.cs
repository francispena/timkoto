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

        public async Task<ResponseBase> AddUser(AddUserRequest request, Guid traceId, List<string> messages)
        {
            var addUserResponse = new AddUserResponse();

            var registrationCode = await _persistService.FindOne<RegistrationCode>(_ => _.Code == request.RegistrationCode && _.IsActive);
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

            var user = new User
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                IsActive = true,
                OperatorId = registrationCode.OperatorId,
                AgentId = registrationCode.AgentId,
                UserType = registrationCode.UserType
            };

            var result = await _persistService.Save(user);

            if (result > 0)
            {
                registrationCode.IsActive = false;
                await _persistService.Update(registrationCode);

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
