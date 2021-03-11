using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class AddUserResponse: ResponseBase
    {
        public static AddUserResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            AddNewUserResult result)
        {
            return new AddUserResponse
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

        private static string GetCodeDescription(AddNewUserResult result)
        {
            switch (result)
            {
                case AddNewUserResult.NewUserCreated:
                    return "New user created.";
                case AddNewUserResult.EmailAddressExists:
                    return "Email address exists.";
                case AddNewUserResult.InvalidRegistrationCode:
                    return "Invalid Registration Code.";
            }

            return "";
        }
    }
}
