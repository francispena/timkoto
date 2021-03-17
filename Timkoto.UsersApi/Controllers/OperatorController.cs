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
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        public OperatorController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [Route("Agents/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> Agents([FromRoute] long operatorId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _operatorService.GetAgents(operatorId, messages);

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

        [Route("Agents/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> ContestAgents([FromRoute] long operatorId, [FromRoute] string gameDate)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _operatorService.GetContestAgents(operatorId, gameDate, messages);

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
