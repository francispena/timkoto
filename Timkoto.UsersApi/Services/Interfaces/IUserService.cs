using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<GenericResponse> AddUser(AddUserRequest request, List<string> messages);

        Task<GenericResponse> CheckUserName(AddUserRequest request, List<string> messages);

        Task<GenericResponse> UpdateUser(UpdateUserRequest request, List<string> messages);
    }
}