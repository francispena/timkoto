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
    public class ContestController : ControllerBase
    {
        private readonly IContestService  _contestService;

        private readonly ILogger _logger;

        private readonly string _className = "ContestController";

        public ContestController(IContestService contestService, ILogger logger)
        {
            _contestService = contestService;
            _logger = logger;
        }

        [Route("Teams/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> GetGames([FromRoute] long contestId)
        {
            var member = $"{_className}.GetGames";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - contestId:{contestId}");

            GenericResponse result;

            try
            {
                result = await _contestService.GetGames(contestId, messages);
                messages.AddWithTimeStamp($"_contestService.GetGames - {JsonConvert.SerializeObject(result)}");

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

        [Route("Players/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> GetPlayers([FromRoute] long contestId)
        {
            var member = $"{_className}.GetPlayers";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - contestId:{contestId}");

            GenericResponse result;

            try
            {
                result = await _contestService.GetPlayers(contestId, messages);
                messages.AddWithTimeStamp($"_contestService.GetPlayers - {JsonConvert.SerializeObject(result)}");

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

        [Route("LineUp")]
        [HttpPost]
        public async Task<IActionResult> SubmitLineUp([FromBody] LineUpRequest request)
        {
            var member = $"{_className}.SubmitLineUp";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");

            GenericResponse result;

            try
            {
                result = await _contestService.SubmitLineUp(request, messages);
                messages.AddWithTimeStamp($"_contestService.SubmitLineUp - {JsonConvert.SerializeObject(result)}");

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

        [Route("PrizePool/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> GetPrizePool([FromRoute] long operatorId)
        {
            var member = $"{_className}.GetPrizePool";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}");

            GenericResponse result;

            try
            {
                result = await _contestService.PrizePool(operatorId, messages);
                messages.AddWithTimeStamp($"_contestService.PrizePool - {JsonConvert.SerializeObject(result)}");

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

        [Route("TeamRanks/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> GetTeamRanks([FromRoute] long operatorId)
        {
            var member = $"{_className}.GetTeamRanks";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}");

            GenericResponse result;

            try
            {
                result = await _contestService.TeamRanks(operatorId, messages);
                messages.AddWithTimeStamp($"_contestService.TeamRanks - {JsonConvert.SerializeObject(result)}");

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

        [Route("TeamHistoryRanks/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> TeamHistoryRanks([FromRoute] long operatorId, string gameDate)
        {
            var member = $"{_className}.TeamHistoryRanks";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - operatorId:{operatorId}/gameDate:{gameDate}");

            GenericResponse result;

            try
            {
                result = await _contestService.TeamHistoryRanks(operatorId, gameDate , messages);
                messages.AddWithTimeStamp($"_contestService.TeamHistoryRanks - {JsonConvert.SerializeObject(result)}");

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
