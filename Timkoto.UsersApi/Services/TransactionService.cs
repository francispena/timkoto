using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
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

        public async Task<GenericResponse> AddTransaction(AddTransactionRequest request, bool limitAmount,  List<string> messages)
        {
            var lastTransaction = await
                _persistService.FindLast<Transaction>(_ => _.UserId == request.UserId, _ => _.CreateDateTime);

            if (request.TransactionType.ToString().Contains("Credit"))
            {
                request.Amount = Math.Abs(request.Amount) * -1;
            }
            else if (request.TransactionType.ToString().Contains("Debit"))
            {
                request.Amount = Math.Abs(request.Amount);

                if (limitAmount && request.Amount > 300m)
                {
                    return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AmountNotAccepted);
                }
            }

            var newTransaction = new Transaction
            {
                Amount = request.Amount,
                OperatorId = request.OperatorId,
                AgentId = request.AgentId,
                UserType = request.UserType,
                TransactionType = request.TransactionType,
                UserId = request.UserId,
                Balance = lastTransaction?.Balance + request.Amount ?? request.Amount
            };

            var user = await _persistService.FindOne<User>(_ => _.Id == request.UserId);
            
            user.Points = newTransaction.Balance;

            var dbSession = _persistService.GetSession();

            var result = 0L;
            var tx = dbSession.BeginTransaction();
            try
            {
                await dbSession.UpdateAsync(user);
                result = (long)await dbSession.SaveAsync(newTransaction);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
            }
            
            return result > 0
                 ? GenericResponse.Create(true, HttpStatusCode.OK, Results.TransactionAdded)
                 : GenericResponse.Create(true, HttpStatusCode.Forbidden, Results.AddTransactionFailed);
        }

        public async Task<GenericResponse> Balance(long userId, List<string> messages)
        {
            var lastTransaction = await
                _persistService.FindLast<Transaction>(_ => _.UserId == userId, _ => _.CreateDateTime);
            
            var genericResponse  = GenericResponse.Create(true, HttpStatusCode.OK, Results.TransactionFound); 
            if (lastTransaction != null)
            {
                genericResponse.Data = new
                {
                    lastTransaction.Balance
                };
            }
            else
            {
                genericResponse.Data = new
                {
                    Balance = 0m
                };
            }
            
            return genericResponse;
        }
    }
}
