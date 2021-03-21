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

        [Route("Teams/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> GetGames([FromRoute] string gameDate)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.GetGames(gameDate, messages);

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

        [Route("Players/{gameDate}")]
        [HttpGet]
        public async Task<IActionResult> GetPlayers([FromRoute] string gameDate)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.GetPlayers(gameDate, messages);

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

        [Route("PrizePool")]
        [HttpGet]
        public async Task<IActionResult> GetPrizePool()
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _contestService.PrizePool(messages);

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
