using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;

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
    }
}
