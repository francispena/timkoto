using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<GenericResponse> AddTransaction(AddTransactionRequest request, List<string> messages);
    }
}