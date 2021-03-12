using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<ResponseBase> AddTransaction(AddTransactionRequest request, Guid traceId, List<string> messages);
    }
}