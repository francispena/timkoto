using System;
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

        public string Tag { get; set; }

        public static GenericResponse Create(bool isSuccess, HttpStatusCode statusCode,
            Results result)
        {
            return new GenericResponse
            {
                IsSuccess = isSuccess,
                ResponseCode = statusCode,
                ResponseMessage = statusCode.ToString(),
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
                    return "Invalid user name and password.";
                case Results.AuthenticationSucceeded:
                    return "Authentication succeeded.";
                case Results.ChangePasswordFailed:
                    return "ChangePassword failed, please contact your agent.";
                case Results.ChangePasswordSucceeded:
                    return "ChangePassword succeeded.";
                case Results.GameNotFound:
                    return "Game not found.";
                case Results.NoContestTeamFound:
                    return "No playing team found in contest.";
                case Results.ContestTeamFound:
                    return "Playing teams found in contest.";
                case Results.NoLineUpToUpdate:
                    return "There is no lineup to update.";
                case Results.NotEnoughPoints:
                    return "Not enough points to play, please contact your agent.";
                case Results.NoContestFound:
                    return "Invalid contest.";
                case Results.ProcessingTransactionFailed:
                    return "Processing transaction failed.";
                case Results.PlayerLineUpCreated:
                    return "Player lineup created.";
                case Results.InvalidLineUpCount:
                    return "Please select 9 players.";
                case Results.InvalidOperatorId:
                    return "Invalid operator Id.";
                case Results.InvalidAgentID:
                    return "Invalid agent Id.";
                case Results.InvalidContestId:
                    return "Invalid contest Id.";
                case Results.AuthenticationError:
                    return "Login process error, please contact your agent.";
                case Results.TeamSubmissionNotAccepted:
                    return "Team submission not accepted, contest has started.";
                case Results.AmountNotAccepted:
                    return "Amount not accepted.";
                case Results.NoTeamFound:
                    return "No team found for player.";
                case Results.TeamsFound:
                    return "Team found for player.";
                case Results.NoTeamPlayersFound:
                    return "No players found.";
                case Results.TeamPlayersFound:
                    return "Players found.";
                case Results.AgentPointsFound:
                    return "Agent points found.";
                case Results.NoAgentPointsFound:
                    return "No agent points found.";
                case Results.PrizePoolFound:
                    return "Prize pool found.";
                case Results.PrizePoolNotSet:
                    return "No prize pool set for the contest.";
                case Results.EmailSent:
                    return "Email sent.";
                case Results.EmailSendingFailed:
                    return "Sending of email failed.";
                case Results.PlayerNotFound:
                    return "Player record not found.";
                case Results.InvalidResetCode:
                    return "Invalid password reset code.";
                case Results.UserNameExists:
                    return "Username is not available, please set a different user name.";
                case Results.NoTransactionFound:
                    return "No transactions found.";
                case Results.ContestPackageNotAssigned:
                    return "Contest package not assigned.";
                case Results.NoContestPackage:
                    return "No Contest package defined.";
                case Results.ExceededSalary:
                    return "Players' total salary exceeded the salary cap.";
                case Results.UserNameAvailable:
                    return "Username is available.";
            }

            return "";
        }
    }
}
