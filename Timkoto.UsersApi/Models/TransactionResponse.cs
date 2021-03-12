using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class TransactionResponse : ResponseBase
    {
        public static TransactionResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            AddTransactionResult result)
        {
            return new TransactionResponse
            {
                IsSuccess = isSuccess,
                TraceId = traceId.ToString(),
                ResponseCode = statusCode,
                Result = new
                {
                    Code = result.ToString(),
                    Description = GetCodeDescription(result)
                }
            };
        }

        private static string GetCodeDescription(AddTransactionResult result)
        {
            switch (result)
            {
                case AddTransactionResult.TransactionAdded:
                    return "Transaction Added";
            }

            return "";
        }
    }
}
