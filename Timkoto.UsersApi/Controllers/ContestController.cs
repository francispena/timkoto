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
    public class ContestController : ControllerBase
    {
        private readonly IContestService  _contestService;

        public ContestController(IContestService contestService)
        {
            _contestService = contestService;
        }

        [Route("Teams/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> GetGames([FromRoute] long contestId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.GetGames(contestId, messages);

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

        [Route("Players/{contestId}")]
        [HttpGet]
        public async Task<IActionResult> GetPlayers([FromRoute] long contestId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.GetPlayers(contestId, messages);

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

        [Route("LineUp")]
        [HttpPost]
        public async Task<IActionResult> SumbitLineUp([FromBody] LineUpRequest request)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.SubmitLineUp(request, messages);

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

        [Route("PrizePool/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> GetPrizePool([FromRoute] long operatorId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.PrizePool(operatorId, messages);

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

        [Route("TeamRanks/{operatorId}")]
        [HttpGet]
        public async Task<IActionResult> GetTeamRanks([FromRoute] long operatorId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.TeamRanks(operatorId, messages);

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

        [Route("TeamHistoryRanks/{operatorId}/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> TeamHistoryRanks([FromRoute] long operatorId, string gameDate)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.TeamHistoryRanks(operatorId, gameDate , messages);

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
