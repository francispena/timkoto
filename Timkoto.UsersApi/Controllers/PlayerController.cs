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
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [Route("{userId}")]
        [HttpGet]
        public async Task<IActionResult> Players([FromRoute] long userId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetUser(userId, messages);

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

        [Route("{operatorId}/{agentId}")]
        [HttpGet]
        public async Task<IActionResult> Players([FromRoute] long operatorId, [FromRoute] long agentId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetPlayers(operatorId, agentId, messages);

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

        [Route("TeamsInContest/{userId}/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> Teams([FromRoute] long userId, [FromRoute] long contestId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamsInContest(userId, contestId, messages);

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


        [Route("TeamHistory/{userId}")]
        [HttpGet]
        public async Task<IActionResult> AllTeams([FromRoute] long userId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamsHistory(userId, messages);

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


        [Route("TeamPlayerStats/{playerTeamId}")]
        [HttpGet]
        public async Task<IActionResult> TeamPlayerStats([FromRoute] long playerTeamId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetTeamPlayerStats(playerTeamId, messages);

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

        [Route("GetHomePageData/{operatorId}/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetHomePageData([FromRoute] long operatorId, [FromRoute] long userId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _playerService.GetHomePageData(operatorId, userId, messages);

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
