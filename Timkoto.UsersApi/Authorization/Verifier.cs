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

        public string GetEmail(string idToken)
        {
            var jwts = idToken.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (jwts.Length != 3)
            {
                throw new NotAuthorizedException("");
            }

            var idTokenPayloadStringBase64String = Convert.FromBase64String($"{jwts[1]}{(jwts[1].Length % 2 == 0 ? "==" : "=")}");
            var decodedString = Encoding.UTF8.GetString(idTokenPayloadStringBase64String);

            var idTokenPayload = JsonConvert.DeserializeObject<IdTokenPayload>(decodedString);

            return idTokenPayload?.CognitoUsername;
        }
    }
}
