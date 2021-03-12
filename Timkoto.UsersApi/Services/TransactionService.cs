using System;
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

        public async Task<ResponseBase> AddTransaction(AddTransactionRequest request, Guid traceId, List<string> messages)
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
                 ? TransactionResponse.Create(true, traceId, HttpStatusCode.OK, AddTransactionResult.TransactionAdded)
                 : TransactionResponse.Create(true, traceId, HttpStatusCode.Forbidden,
                     AddTransactionResult.AddTransactionFailed);
        }
    }
}
