using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.BaseClasses;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContestController : ControllerBase
    {
        private readonly IContestService  _contestService;

        private readonly IAppConfig _appConfig;

        private readonly IVerifier _verifier;

        public ContestController(IContestService contestService, IAppConfig appConfig, IVerifier verifier)
        {
            _contestService = contestService;
            _appConfig = appConfig;
            _verifier = verifier;
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
                //if (_appConfig.IsProduction)
                //{
                //    var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];

                //    var jwt = JsonConvert.DeserializeObject<JWToken>(httpOnlyAccessToken);

                //    if (jwt == null)
                //    {
                //        return StatusCode(401, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.Unauthorized));
                //    }

                //    var verified = await _verifier.VerifyAccessToken(request.LineUpTeam.UserId, jwt.AccessToken);
                //    if (!verified)
                //    {
                //        return StatusCode(401, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.Unauthorized));
                //    }
                //}

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
