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
        public async Task<IActionResult> Players([FromRoute] long userId, [FromHeader] Guid traceId)
        {
            var messages = new List<string>();
            ResponseBase result;

            try
            {
                result = await _playerService.GetPlayer(userId, traceId, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = ResponseBase.CreateErrorResponse(ex);

                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
