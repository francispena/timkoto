using System.Threading.Tasks;

namespace Timkoto.UsersApi.Authorization.Interfaces
{
    public interface ICognitoUserStore
    {
        Task<bool> CreateAsync(string userName, string password);

        Task<string> AuthenticateAsync(string userName, string password);
    }
}