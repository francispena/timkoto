using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class GenerateCodeResponse : ResponseBase
    {
        public static GenerateCodeResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            GenerateCodeResult result)
        {
            return new GenerateCodeResponse
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

        private static string GetCodeDescription(GenerateCodeResult result)
        {
            switch (result)
            {
                case GenerateCodeResult.CodeCreated:
                    return "Code created.";
                case GenerateCodeResult.InvalidUserId:
                    return "Invalid User Id.";
            }

            return "";
        }
    }
}
