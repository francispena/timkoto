using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/registration/v1/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly ICognitoUserStore _cognitoUserStore;

        private readonly IRegistrationCodeService _registrationCodeService;

        private readonly IEmailService _emailService;

        private readonly ILogger _logger;

        private readonly string _className = "UserController";

        public UserController(IUserService userService, ICognitoUserStore cognitoUserStore,
            IRegistrationCodeService registrationCodeService, IEmailService emailService, ILogger logger)
        {
            _userService = userService;
            _cognitoUserStore = cognitoUserStore;
            _registrationCodeService = registrationCodeService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
        {
            var member = $"{_className}.AddUser";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");
            
            GenericResponse result;
            
            try
            {
                result = await _userService.AddUser(request, messages);
                messages.AddWithTimeStamp($"_userService.AddUser - {JsonConvert.SerializeObject(result)}");

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");
                
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var member = $"{_className}.Authenticate";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            try
            {
                var authenticationResult =
                    await _cognitoUserStore.AuthenticateAsync(request.Email, request.Password, messages);
                messages.AddWithTimeStamp($"_cognitoUserStore.AuthenticateAsync - {JsonConvert.SerializeObject(authenticationResult)}");

                if (authenticationResult.IsSuccess)
                {
                    Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                    Response.Cookies.Append("HttpOnlyAccessToken", JsonConvert.SerializeObject(authenticationResult.Jwt), new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = true,
                        IsEssential = true,
                        Expires = DateTime.Now.AddMonths(1),
                        Secure = false,
                        Domain = "timkoto.com"
                    });

                    //authenticationResult.Jwt = null;

                    return Ok(authenticationResult);
                }
                else
                {
                    return StatusCode(403, authenticationResult);
                }
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                var result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("changePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var member = $"{_className}.ChangePassword";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            try
            {
                var changePasswordResult = await _cognitoUserStore.ChangePasswordAsync(request, messages);
                messages.AddWithTimeStamp($"_cognitoUserStore.ChangePasswordAsync - {JsonConvert.SerializeObject(changePasswordResult)}");

                if (changePasswordResult.IsSuccess)
                {
                    return Ok(changePasswordResult);
                }
                else
                {
                    return StatusCode(403, changePasswordResult);
                }
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                var result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("sendPasswordResetEmail")]
        [HttpPost]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] PasswordResetRequest request)
        {
            var member = $"{_className}.SendPasswordResetEmail";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            GenericResponse retVal;

            try
            {
                var user = await _registrationCodeService.GenerateResetPasswordCode(request.EmailAddress, messages);
                if (user != null)
                {
                    messages.AddWithTimeStamp($"_registrationCodeService.GenerateResetPasswordCode - {JsonConvert.SerializeObject(user)}");
                    var result = await _emailService.SendPasswordResetCode(user, messages);
                    messages.AddWithTimeStamp($"_emailService.SendPasswordResetCode - {JsonConvert.SerializeObject(result)}");

                    if (result)
                    {
                        retVal = GenericResponse.Create(true, HttpStatusCode.OK, Results.EmailSent);
                        return Ok(retVal);
                    }
                    else
                    {
                        retVal = GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.EmailSendingFailed);
                        return StatusCode(403, retVal);
                    }
                }
                else
                {
                    retVal = GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidUserId);
                    return StatusCode(403, retVal);
                }
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                retVal = GenericResponse.CreateErrorResponse(ex);

                return StatusCode(500, retVal);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("checkUserName")]
        [HttpPost]
        public async Task<IActionResult> CheckUserName([FromBody] AddUserRequest request)
        {
            var member = $"{_className}.CheckUserName";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            GenericResponse result;

            try
            {
                result = await _userService.CheckUserName(request, messages);
                messages.AddWithTimeStamp($"_userService.CheckUserName - {JsonConvert.SerializeObject(result)}");

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("refreshToken")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            var member = $"{_className}.RefreshToken";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member}");

            try
            {
                var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];

                if (string.IsNullOrWhiteSpace(httpOnlyAccessToken))
                {
                    return StatusCode(403, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.NoTokenFound));
                }
                
                var jwt = JsonConvert.DeserializeObject<JWToken>(httpOnlyAccessToken);

                if (jwt == null)
                {
                    return StatusCode(401, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.Unauthorized));
                }

                var refreshTokenResult = await _cognitoUserStore.RefreshToken(httpOnlyAccessToken, messages);

                if (refreshTokenResult.IsSuccess)
                {
                    Response.Cookies.Append("HttpOnlyAccessToken", JsonConvert.SerializeObject(refreshTokenResult.Jwt), new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = false,
                        IsEssential = true,
                        Expires = DateTime.Now.AddMonths(1),
                        Secure = true
                    });

                    //refreshTokenResult.Jwt = null;

                    return Ok(refreshTokenResult);
                }
                else
                {
                    return StatusCode(403, refreshTokenResult);
                }
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                var result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }
        
    }
}
