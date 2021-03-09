using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
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
            var messages = new List<string>();

            var user = new User
            {
                UserType = UserType.Player,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                UserName = newUser.UserName,
                IsActive = true
            };

            var result = await _userService.AddUser(user, traceId, messages);
            
            if (result.ResponseCode == HttpStatusCode.OK)
            {
                return Ok(result);
            }
            else if (result.ResponseCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(403, result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }
    }
}
