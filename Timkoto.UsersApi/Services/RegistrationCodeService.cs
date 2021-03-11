using System;
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

        public async Task<ResponseBase> Generate(long userId, Guid traceId, List<string> messages)
        {
            GenerateCodeResponse generateCodeResponse;

            var user = await _persistService.FindOne<User>(_ => _.Id == userId);

            if (user == null)
            {
                generateCodeResponse = GenerateCodeResponse.Create(false, traceId, HttpStatusCode.Forbidden,
                    GenerateCodeResult.InvalidUserId);

                return generateCodeResponse;
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

            var result = await _persistService.Save(registrationCode);

            generateCodeResponse = GenerateCodeResponse.Create(true, traceId, HttpStatusCode.OK,
                GenerateCodeResult.CodeCreated);
            
            generateCodeResponse.Data = new {
                Code = code
            };

            return generateCodeResponse;
        }
    }
}
