﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class RegistrationCodeService : IRegistrationCodeService
    {
        private readonly IPersistService _persistService;

        public RegistrationCodeService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<GenericResponse> Generate(long userId, List<string> messages)
        {
            GenericResponse genericResponse;

            var user = await _persistService.FindOne<User>(_ => _.Id == userId && _.UserType != UserType.Player);

            if (user == null)
            {
                genericResponse = GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidUserId);

                return genericResponse;
            }

            var code = Guid.NewGuid().ToString("N");
            var toEncodeAsBytes = System.Text.Encoding.ASCII.GetBytes(code);
            code = HttpUtility.UrlEncode(toEncodeAsBytes);

            var registrationCode = new RegistrationCode
            {
                Code = code,
                IsActive = true,
                UserName = user.UserName,
            };

            switch (user.UserType)
            {
                case UserType.Operator:
                    registrationCode.OperatorId = user.Id;
                    registrationCode.UserType = UserType.Agent;
                    break;
                case UserType.Agent:
                    registrationCode.AgentId = user.Id;
                    registrationCode.OperatorId = user.OperatorId;
                    registrationCode.UserType = UserType.Player;
                    break;
                case UserType.Player:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _persistService.Save(registrationCode);

            genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.CodeCreated);
            
            var configuration = Startup.Configuration;

            genericResponse.Data = new {
                Code = $"{configuration["RegistrationLink"]}{code}"
            };

            return genericResponse;
        }

        public async Task<User> GenerateResetPasswordCode(string requestEmailAddress, List<string> messages)
        {
            var user = await _persistService.FindOne<User>(_ => _.IsActive && _.Email == requestEmailAddress);
            if (user == null)
            {
                return null;
            }
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            user.PasswordResetCode = code;
            user.UpdateDateTime = DateTime.UtcNow;

            var updateResult = await _persistService.Update(user);
            return updateResult ? user : null;
        }
    }
}
