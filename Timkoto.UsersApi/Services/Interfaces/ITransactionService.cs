using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<GenericResponse> AddTransaction(AddTransactionRequest request, bool limit, List<string> messages);

        Task<GenericResponse> Balance(long userId, List<string> messages);
        
        Task<GenericResponse> History(long userId, List<string> messages);
    }
}