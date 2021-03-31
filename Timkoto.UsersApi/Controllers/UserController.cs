using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
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

        private readonly ILambdaContext _lambdaContext;

        public UserController(IUserService userService, ICognitoUserStore cognitoUserStore,
            IRegistrationCodeService registrationCodeService, IEmailService emailService)
        {
            _userService = userService;
            _cognitoUserStore = cognitoUserStore;
            _lambdaContext = Startup.LambdaContext;
            _registrationCodeService = registrationCodeService;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
        {
            var messages = new List<string> { "UserController.AddUser", $"request - {JsonConvert.SerializeObject(request)}"};
            GenericResponse result;
            
            //validate token against DB
            var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];

            try
            {
                result = await _userService.AddUser(request, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }
        }

        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var messages = new List<string> { "UserController.Authenticate", $"request - {JsonConvert.SerializeObject(request)}" };

            try
            {
                var authenticationResult =
                    await _cognitoUserStore.AuthenticateAsync(request.Email, request.Password, messages);

                if (authenticationResult.IsSuccess)
                {
                    Response.Cookies.Append("HttpOnlyAccessToken", authenticationResult.Tag, new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = true,
                        IsEssential = true,
                        Expires = DateTime.Now.AddMonths(1),
                        Secure = true
                    });

                    authenticationResult.Tag = null;

                    return Ok(authenticationResult);
                }
                else
                {
                    return StatusCode(403, authenticationResult);
                }
            }
            catch (Exception ex)
            {
                var result = GenericResponse.CreateErrorResponse(ex);

                return StatusCode(500, result);
            }
            finally
            {
                _lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }
        }

        [Route("changePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var messages = new List<string> { "UserController.ChangePassword", $"request - {JsonConvert.SerializeObject(request)}" };

            try
            {
                var changePasswordResult = await _cognitoUserStore.ChangePasswordAsync(request, messages);

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
                var result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }
        }

        [Route("sendPasswordResetEmail")]
        [HttpPost]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] PasswordResetRequest request)
        {
            var messages = new List<string>();
            GenericResponse retVal;

            try
            {
                var user = await _registrationCodeService.GenerateResetPasswordCode(request.EmailAddress, messages);
                if (user != null)
                {
                    var result = await _emailService.SendPasswordResetCode(user, messages);
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
                retVal = GenericResponse.CreateErrorResponse(ex);

                return StatusCode(500, retVal);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("checkUserName")]
        [HttpPost]
        public async Task<IActionResult> CheckUserName([FromBody] AddUserRequest request)
        {
            var messages = new List<string> { "UserController.checkUserName", $"request - {JsonConvert.SerializeObject(request)}" };
            GenericResponse result;

            //validate token against DB
            var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];

            try
            {
                result = await _userService.CheckUserName(request, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }
        }
    }
}
