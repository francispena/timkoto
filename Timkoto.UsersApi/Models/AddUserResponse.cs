using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class AddUserResponse: ResponseBase
    {
        public static AddUserResponse Create(bool isSuccess, Guid traceId, HttpStatusCode statusCode,
            AddNewUserResult addNewUserResult)
        {
            return new AddUserResponse
            {
                IsSuccess = isSuccess,
                TraceId = traceId.ToString(),
                ResponseCode = statusCode,
                Result = new
                {
                    Code = addNewUserResult.ToString(),
                    Description = GetCodeDescription(addNewUserResult)
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
            }

            return "";
        }
    }
}
