﻿using System;
using System.Net;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class GenericResponse
    {
        public dynamic Result { get; set; }

        public bool IsSuccess { get; set; }
        
        public HttpStatusCode ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionStackTrace { get; set; }

        public dynamic Data { get; set; }

        public static GenericResponse Create(bool isSuccess, HttpStatusCode statusCode,
            Results result)
        {
            return new GenericResponse
            {
                IsSuccess = isSuccess,
                ResponseCode = statusCode,
                Result = new
                {
                    Code = result.ToString(),
                    Description = GetCodeDescription(result)
                }
            };
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static GenericResponse CreateErrorResponse(Exception ex)
        {
            return new GenericResponse
            {
                IsSuccess = false,
                ResponseCode = HttpStatusCode.InternalServerError,
                ResponseMessage = HttpStatusCode.InternalServerError.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionStackTrace = ex.StackTrace
            };
        }

        private static string GetCodeDescription(Results result)
        {
            switch (result)
            {
                case Results.AgentsFound:
                    return "Agents found.";
                case Results.NoAgentFound:
                    return "No agent found.";
                case Results.NewUserCreated:
                    return "New user created.";
                case Results.EmailAddressExists:
                    return "Email address exists.";
                case Results.InvalidRegistrationCode:
                    return "Invalid registration code, please contact your agent.";
                case Results.AccountCreationError:
                    return "Account creation error, please contact your agent.";
                case Results.CodeCreated:
                    return "Code created.";
                case Results.InvalidUserId:
                    return "Invalid User Id.";
                case Results.PlayerFound:
                    return "Players found.";
                case Results.PlayersFound:
                    return "Players found.";
                case Results.NoPlayerFound:
                    return "No players found.";
                case Results.TransactionAdded:
                    return "Transaction Added.";
                case Results.AuthenticationFailed:
                    return "Invalid user and password.";
                case Results.AuthenticationSucceeded:
                    return "Authentication succeeded.";
                case Results.ChangePasswordFailed:
                    return "ChangePassword failed, please contact your agent.";
                case Results.ChangePasswordSucceeded:
                    return "ChangePassword succeeded.";
            }

            return "";
        }
    }
}
