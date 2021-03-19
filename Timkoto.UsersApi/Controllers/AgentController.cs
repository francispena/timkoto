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

        [Route("Players/{operatorId}/{agentId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> ContestAgents([FromRoute] long operatorId, [FromRoute] long agentId, [FromRoute] string gameDate)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _agentService.GetContestPlayers(operatorId, agentId, gameDate, messages);

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

        [Route("AgentPoints/{agentId}")]
        [HttpGet]
        public async Task<IActionResult> AgentPoints([FromRoute] long agentId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _agentService.GetAgentPoints(agentId, messages);

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
