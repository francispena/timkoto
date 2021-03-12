using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class GetPlayerResponse : ResponseBase
    {
        public static GetPlayerResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            GetPlayerResult result)
        {
            return new GetPlayerResponse
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

        private static string GetCodeDescription(GetPlayerResult result)
        {
            switch (result)
            {
                case GetPlayerResult.PlayerFound:
                    return "Players found.";
                case GetPlayerResult.NoPlayerFound:
                    return "No players found.";
            }

            return "";
        }
    }
}
