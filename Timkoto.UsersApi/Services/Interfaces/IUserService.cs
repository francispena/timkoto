using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<GenericResponse> AddUser(AddUserRequest request, List<string> messages);
    }
}