using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/utility/v1/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IHttpService _httpService;

        private readonly IPersistService _persistService;

        public UtilityController(IHttpService httpService, IPersistService persistService)
        {
            _httpService = httpService;
            _persistService = persistService;
        }

        [Route("GetTeams")]
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var messages = new List<string>();
            ResponseBase result;

            try
            {
                var response = await _httpService.GetAsync<RapidApiTeams>("https://api-nba-v1.p.rapidapi.com/teams/league/standard",
                    new Dictionary<string, string>
                    {
                        {"x-rapidapi-key","052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                    });

                var teams = new List<NbaTeam>();
                foreach (var apiTeam in response.Api.teams)
                {
                    if (apiTeam.leagues.standard.confName != "Intl")
                    {
                        var team = new NbaTeam
                        {
                            City = apiTeam.city,
                            FullName = apiTeam.fullName,
                            Id = apiTeam.teamId,
                            Logo = apiTeam.logo,
                            NickName = apiTeam.nickname
                        };

                        teams.Add(team);
                    }
                }

                var retVal = await _persistService.BatchSave(teams);

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                result = ResponseBase.CreateErrorResponse(ex);
                result.Data = messages;
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
