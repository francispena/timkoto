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
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        private readonly ILogger _logger;

        private readonly string _className = "PlayerController";
        
        public PlayerController(IPlayerService playerService, ILogger logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [Route("{userId}")]
        [HttpGet]
        public async Task<IActionResult> Players([FromRoute] long userId)
        {
            var member = $"{_className}.Players";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - userId:{userId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetUser(userId, messages);
                messages.AddWithTimeStamp($"_playerService.GetUser - {JsonConvert.SerializeObject(result)}");

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

        [Route("{operatorId}/{agentId}")]
        [HttpGet]
        public async Task<IActionResult> Players([FromRoute] long operatorId, [FromRoute] long agentId)
        {
            var member = $"{_className}.Players";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/agentId:{agentId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetPlayers(operatorId, agentId, messages);
                messages.AddWithTimeStamp($"_playerService.GetPlayers - {JsonConvert.SerializeObject(result)}");

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

        [Route("TeamsInContest/{userId}/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> Teams([FromRoute] long userId, [FromRoute] long contestId)
        {
            var member = $"{_className}.Teams";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - userId:{userId}/contestId:{contestId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamsInContest(userId, contestId, messages);
                messages.AddWithTimeStamp($"_playerService.GetTeamsInContest - {JsonConvert.SerializeObject(result)}");

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


        [Route("TeamHistory/{userId}")]
        [HttpGet]
        public async Task<IActionResult> TeamHistory([FromRoute] long userId)
        {
            var member = $"{_className}.TeamHistory";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - userId:{userId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamsHistory(userId, messages);
                messages.AddWithTimeStamp($"_playerService.GetTeamsHistory - {JsonConvert.SerializeObject(result)}");

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


        [Route("TeamPlayerStats/{playerTeamId}")]
        [HttpGet]
        public async Task<IActionResult> TeamPlayerStats([FromRoute] long playerTeamId)
        {
            var member = $"{_className}.TeamPlayerStats";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - playerTeamId:{playerTeamId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamPlayerStats(playerTeamId, messages);
                messages.AddWithTimeStamp($"_playerService.GetTeamPlayerStats - {JsonConvert.SerializeObject(result)}");

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

        [Route("GetHomePageData/{operatorId}/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetHomePageData([FromRoute] long operatorId, [FromRoute] long userId)
        {
            var member = $"{_className}.GetHomePageData";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/userId:{userId}");
            GenericResponse result;

            try
            {
                result = await _playerService.GetHomePageData(operatorId, userId, messages);
                messages.AddWithTimeStamp($"_playerService.GetHomePageData - {JsonConvert.SerializeObject(result)}");

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
