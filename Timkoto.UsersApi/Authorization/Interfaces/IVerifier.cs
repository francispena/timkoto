using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization.Interfaces
{
    public interface IVerifier
    {
        Task<bool> VerifyAccessToken(long Id, string accessToken);

        Task<bool> VerifyTransactionRequest(string idToken, AddTransactionRequest addTransactionRequest);
    }
}