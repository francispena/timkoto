using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RegistrationCodeController : ControllerBase
    {
        private readonly IRegistrationCodeService _registrationCodeService;

        public RegistrationCodeController(IRegistrationCodeService registrationCodeService)
        {
            _registrationCodeService = registrationCodeService;
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
    }
}
