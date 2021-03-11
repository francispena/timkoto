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
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [Route("{operatorId}/{agentId}")]
        [HttpGet]
        public async Task<IActionResult> Players([FromRoute] long operatorId, [FromRoute] long agentId, [FromHeader] Guid traceId)
        {
            var messages = new List<string>();
            ResponseBase result;

            try
            {
                result = await _agentService.GetPlayers(operatorId, agentId, traceId, messages);

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
