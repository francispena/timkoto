using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class UserService : IUserService
    {
        private readonly IPersistService _persistService;

        private readonly ICognitoUserStore _cognitoUserStore;

        public UserService(IPersistService persistService, ICognitoUserStore cognitoUserStore)
        {
            _persistService = persistService;
            _cognitoUserStore = cognitoUserStore;
        }

        public async Task<GenericResponse> AddUser(AddUserRequest request, List<string> messages)
        {
            var registrationCode = await _persistService.FindOne<RegistrationCode>(_ => _.Code == request.RegistrationCode && _.IsActive);
            if (registrationCode == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidRegistrationCode);
            }
            if (DateTime.UtcNow.Subtract(registrationCode.CreateDateTime).TotalMinutes > 120)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidRegistrationCode);
            }

            var existingUser = await _persistService.FindOne<User>(_ =>
                _.UserName == request.UserName && _.OperatorId == registrationCode.OperatorId);
            
            if (existingUser != null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.UserNameExists);
            }

            var genericResponse = new GenericResponse();
            var phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? request.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "")
                : null;

            var user = new User
            {
                Email = request.Email,
                PhoneNumber = phoneNumber,
                UserName = request.UserName,
                IsActive = true,
                OperatorId = registrationCode.OperatorId,
                AgentId = registrationCode.AgentId,
                UserType = registrationCode.UserType,
                Points = 0m
            };

            var result = await _persistService.Save(user);

            if (result > 0)
            {
                var createResult = await _cognitoUserStore.CreateAsync(request.Email, request.Password, messages);
                if (createResult == Results.AccountConfirmedInCognito)
                {
                    registrationCode.IsActive = false;
                    await _persistService.Update(registrationCode);

                    genericResponse =
                        GenericResponse.Create(true, HttpStatusCode.OK, Results.NewUserCreated);
                }
                else
                {
                    await _persistService.Delete(user);

                    genericResponse =
                        GenericResponse.Create(true, HttpStatusCode.Forbidden, createResult);
                }
            }
            else if (result == -1000)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.EmailAddressExists);
            }

            return genericResponse;
        }
    }
}
