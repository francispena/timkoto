﻿using Microsoft.AspNetCore.Mvc;
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
using System.IO;
using System.Linq;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        private readonly ILogger _logger;

        private readonly string _className = "OperatorController";

        public OperatorController(IOperatorService operatorService, ILogger logger)
        {
            _operatorService = operatorService;
            _logger = logger;
        }

        [Route("Agents/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> Agents([FromRoute] long operatorId)
        {
            var member = $"{_className}.Agents";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}");

            GenericResponse result;

            try
            {
                result = await _operatorService.GetAgents(operatorId, messages);
                messages.AddWithTimeStamp($"_operatorService.GetAgents - {JsonConvert.SerializeObject(result)}");

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

        [Route("Agents/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> ContestAgents([FromRoute] long operatorId, [FromRoute] string gameDate)
        {
            var member = $"{_className}.ContestAgents";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/gameDate:{gameDate}");

            GenericResponse result;

            try
            {
                result = await _operatorService.GetContestAgents(operatorId, gameDate, messages);
                messages.AddWithTimeStamp($"_operatorService.GetContestAgents - {JsonConvert.SerializeObject(result)}");
                
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

        [Route("Players/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> ContestAgentPlayers([FromRoute] long operatorId, [FromRoute] string gameDate)
        {
            var member = $"{_className}.ContestAgentPlayers";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/gameDate:{gameDate}");

            GenericResponse result;

            try
            {
                result = await _operatorService.GetContestPlayers(operatorId, gameDate, messages);

                messages.AddWithTimeStamp($"_operatorService.GetContestPlayers - {JsonConvert.SerializeObject(result)}");

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

        [Route("ContestPoints/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> GetContestAgentsForDownload([FromRoute] long operatorId, [FromRoute] string gameDate)
        {
            var member = $"{_className}.GetContestAgentsForDownload";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/gameDate:{gameDate}");

            try
            {
                var contestAgentPointsForDownload = await _operatorService.GetContestAgentsForDownload(operatorId, gameDate, messages);

                var header = "GameDate,OperatorName,AgentName,Collection,Remit,Commission,Prize";
                var rows = string.Join("\r\n", contestAgentPointsForDownload.Select(_ => $"{_.GameDate},{_.OperatorName},{_.AgentName},{_.Collection},{_.Remit},{_.Commission},{_.Prize}"));
                var csv = $"{header}\r\n{rows}";

                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(csv);
                writer.Flush();
                stream.Position = 0;

                var fileName = $"{contestAgentPointsForDownload[0].OperatorName}_{gameDate}.csv";
                var mimeType = "application/csv";
              
                return new FileStreamResult(stream, mimeType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                return StatusCode(500, ex.Message);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }
    }
}
