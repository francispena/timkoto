using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Enumerations;
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

        public RegistrationCodeController(IRegistrationCodeService registrationCodeService, IEmailService emailService)
        {
            _registrationCodeService = registrationCodeService;
            _emailService = emailService;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _registrationCodeService.Generate(id, messages);
                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);

                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("sendEmail")]
        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] EmailRegLinkRequest request)
        {
            var messages = new List<string>();
            GenericResponse retVal;

            try
            {
                var result = await _emailService.SendRegistrationLink(request.EmailAddress, request.Link, messages);

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
                retVal = GenericResponse.CreateErrorResponse(ex);

                return StatusCode(500, retVal);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
