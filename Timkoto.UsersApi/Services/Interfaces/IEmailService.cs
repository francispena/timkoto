using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendRegistrationLink(string emailAddress, string regLink, List<string> messages);
    }
}