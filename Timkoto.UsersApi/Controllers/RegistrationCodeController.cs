using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RegistrationCodeController : ControllerBase
    {
        private readonly IRegistrationCodeService _registrationCodeService;

        private readonly IEmailService _emailService;

        private readonly ILogger _logger;

        private readonly string _className = "RegistrationCodeController";

        private readonly IUserService _userService;

        public RegistrationCodeController(IRegistrationCodeService registrationCodeService, IEmailService emailService, IUserService  userService,ILogger logger)
        {
            _registrationCodeService = registrationCodeService;
            _emailService = emailService;
            _logger = logger;
            _userService = userService;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var member = $"{_className}.Get";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - id:{id}");
            GenericResponse result;

            try
            {
                result = await _registrationCodeService.Generate(id, messages);
                messages.AddWithTimeStamp($"_registrationCodeService.Generate - {JsonConvert.SerializeObject(result)}");

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

        [Route("sendRegistrationLinkEmail")]
        [HttpPost]
        public async Task<IActionResult> SendRegistrationLinkEmail([FromBody] EmailRegLinkRequest request)
        {
            var member = $"{_className}.SendRegistrationLinkEmail";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");
            GenericResponse retVal;

            try
            {
                var result = await _emailService.SendRegistrationLink(request.EmailAddress, request.Link, messages);
                messages.AddWithTimeStamp($"_emailService.SendRegistrationLink - {JsonConvert.SerializeObject(result)}");

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

        [Route("updateUser")]
        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var member = $"{_className}.UpdateUser";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            GenericResponse result;

            try
            {
                result = await _userService.UpdateUser(request, messages);
                messages.AddWithTimeStamp($"_userService.UpdateUser - {JsonConvert.SerializeObject(result)}");

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
    }
}
