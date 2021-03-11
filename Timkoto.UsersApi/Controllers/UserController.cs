﻿using System;
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
            ResponseBase result;

            try
            {
                var user = new User
                {
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    UserName = newUser.UserName,
                    IsActive = true
                };

                result = await _userService.AddUser(user, newUser.RegistrationCode, traceId, messages);

                if (result.ResponseCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                
                return StatusCode(403, result);
            }
            catch (Exception ex)
            {

                result = new ResponseBase
                {
                    IsSuccess = false,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ResponseMessage = HttpStatusCode.InternalServerError.ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace
                };

                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
