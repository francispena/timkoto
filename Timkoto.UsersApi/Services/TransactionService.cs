using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IPersistService _persistService;

        public TransactionService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<GenericResponse> AddTransaction(AddTransactionRequest request, List<string> messages)
        {
            var lastTransaction = await
                _persistService.FindLast<Transaction>(_ => _.UserId == request.UserId, _ => _.CreateDateTime);

            var newTransaction = new Transaction
            {
                Amount = request.Amount,
                OperatorId = request.OperatorId,
                UserType = request.UserType,
                TransactionType = request.TransactionType,
                UserId = request.UserId,
                Balance = lastTransaction?.Balance + request.Amount ?? request.Amount
            };

            var result = await _persistService.Save(newTransaction);

            return result > 0
                 ? GenericResponse.Create(true, HttpStatusCode.OK, Results.TransactionAdded)
                 : GenericResponse.Create(true, HttpStatusCode.Forbidden, Results.AddTransactionFailed);
        }
    }
}
