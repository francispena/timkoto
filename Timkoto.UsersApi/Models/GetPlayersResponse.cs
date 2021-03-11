using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class GetPlayersResponse : ResponseBase
    {
        public static GetPlayersResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            GetPlayersResult result)
        {
            return new GetPlayersResponse
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

        private static string GetCodeDescription(GetPlayersResult result)
        {
            switch (result)
            {
                case GetPlayersResult.PlayersFound:
                    return "Players found.";
                case GetPlayersResult.NoPlayerFound:
                    return "No players found.";
            }

            return "";
        }
    }
}
