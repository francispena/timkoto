using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        private readonly ILogger _logger;

        private readonly string _className = "AgentController";

        public AgentController(IAgentService agentService, ILogger logger)
        {
            _agentService = agentService;
            _logger = logger;
        }

        [Route("Players/{operatorId}/{agentId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> ContestAgents([FromRoute] long operatorId, [FromRoute] long agentId, [FromRoute] string gameDate)
        {
            var member = $"{_className}.ContestAgents";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/agentId:{agentId}/gameDate:{gameDate}");

            GenericResponse result;

            try
            {
                result = await _agentService.GetContestPlayers(operatorId, agentId, gameDate, messages);
                
                messages.AddWithTimeStamp($"_agentService.GetContestPlayers - {JsonConvert.SerializeObject(result)}");

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

        [Route("AgentPoints/{agentId}")]
        [HttpGet]
        public async Task<IActionResult> AgentPoints([FromRoute] long agentId)
        {
            var member = $"{_className}.AgentPoints";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {agentId}");

            GenericResponse result;

            try
            {
                result = await _agentService.GetAgentPoints(agentId, messages);

                messages.AddWithTimeStamp($"_agentService.GetAgentPoints - {JsonConvert.SerializeObject(result)}");

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
