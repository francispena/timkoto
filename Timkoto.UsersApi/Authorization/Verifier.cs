using System;
using System.Text;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider.Model;
using Newtonsoft.Json;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization
{
    public class Verifier : IVerifier
    {
        private readonly IPersistService _persistService;
        public Verifier(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<bool> VerifyAccessToken(long Id, string accessToken)
        {
            var user = await _persistService.FindOne<User>(_ => _.IsActive && _.Id == Id);

            return accessToken == user?.AccessToken;
        }

        public async Task<bool> VerifyTransactionRequest(string idToken, AddTransactionRequest addTransactionRequest)
        {
            var jwts = idToken.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (jwts.Length != 3)
            {
                return false;
            }

            var padding = "";
            if (jwts[1].Length % 4 == 2)
            {
                padding = "==";
            }

            if (jwts[1].Length % 4 == 3)
            {
                padding = "=";
            }

            var idTokenPayloadStringBase64String = Convert.FromBase64String($"{jwts[1]}{padding}");
            var decodedString = Encoding.UTF8.GetString(idTokenPayloadStringBase64String);

            var idTokenPayload = JsonConvert.DeserializeObject<IdTokenPayload>(decodedString);

            var user = await _persistService.FindOne<User>(_ => _.Email == idTokenPayload.CognitoUsername);

            return user.OperatorId == addTransactionRequest.OperatorId &&
                   user.AgentId == addTransactionRequest.AgentId && user.Email == addTransactionRequest.Email;
        }
    }
}
