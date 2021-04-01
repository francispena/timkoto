using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.BaseClasses;
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


        private readonly IAppConfig _appConfig;

        private readonly IVerifier _verifier;

        public RegistrationCodeController(IRegistrationCodeService registrationCodeService, IEmailService emailService, IAppConfig appConfig, IVerifier verifier)
        {
            _registrationCodeService = registrationCodeService;
            _emailService = emailService;
            _appConfig = appConfig;
            _verifier = verifier;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                if (_appConfig.IsProduction)
                {
                    var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];
                    var verified = await _verifier.VerifyAccessToken(id, httpOnlyAccessToken);
                    if (!verified)
                    {
                        return StatusCode(401, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.Unauthorized));
                    }
                }

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

        [Route("sendRegistrationLinkEmail")]
        [HttpPost]
        public async Task<IActionResult> SendRegistrationLinkEmail([FromBody] EmailRegLinkRequest request)
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
