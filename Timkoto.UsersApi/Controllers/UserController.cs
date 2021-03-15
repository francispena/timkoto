using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/registration/v1/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddUserRequest newUser, [FromHeader] Guid traceId)
        {
            var lambdaContext = Startup.LambdaContext;
            var messages = new List<string> { "UserController.Post", $"newUser - {JsonConvert.SerializeObject(newUser)}"};
            lambdaContext.Logger.Log(string.Join("\r\n", messages));

            ResponseBase result;

            try
            {
                result = await _userService.AddUser(newUser, traceId, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = ResponseBase.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
