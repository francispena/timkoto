using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendRegistrationLink(string emailAddress, string regLink, List<string> messages);

        Task<bool> SendPasswordResetCode(User user, List<string> messages);

        Task<bool> SendActivationLink(string emailAddress, string activationLink, string userName, List<string> messages);
    }
}