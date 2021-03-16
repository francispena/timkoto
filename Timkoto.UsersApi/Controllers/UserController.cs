using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
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

        private readonly ILambdaContext _lambdaContext;

        public UserController(IUserService userService, ICognitoUserStore cognitoUserStore)
        {
            _userService = userService;
            _cognitoUserStore = cognitoUserStore;
            _lambdaContext = Startup.LambdaContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
        {
            var messages = new List<string> { "UserController.AddUser", $"request - {JsonConvert.SerializeObject(request)}"};
            GenericResponse result;

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
                var authenticationResult = await _cognitoUserStore.AuthenticateAsync(request.Email, request.Password, messages);

                if (authenticationResult.IsSuccess)
                {
                    return Ok(authenticationResult);
                }
                else
                {
                    return Forbid();
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
                var changePasswordResult = await _cognitoUserStore.ChangePasswordAsync(request.Email, request.Password, messages);

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
    }
}
