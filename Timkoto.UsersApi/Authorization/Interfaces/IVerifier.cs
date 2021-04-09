using System.Threading.Tasks;

namespace Timkoto.UsersApi.Authorization.Interfaces
{
    public interface IVerifier
    {
        Task<bool> VerifyAccessToken(long Id, string accessToken);

        string GetEmail(string idToken);
    }
}