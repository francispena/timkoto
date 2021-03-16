using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Exceptions;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Models.Games;
using Timkoto.UsersApi.Models.Player;
using Timkoto.UsersApi.Models.Team;
using Game = Timkoto.Data.Repositories.Game;


namespace Timkoto.UsersApi.Controllers
{
    [Route("api/utility/v1/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IHttpService _httpService;

        private readonly IPersistService _persistService;

        private readonly ISessionFactory _sessionFactory;

        public UtilityController(IHttpService httpService, IPersistService persistService, ISessionFactory sessionFactory)
        {
            _httpService = httpService;
            _persistService = persistService;
            _sessionFactory = sessionFactory;
        }

        [Route("GetTeams")]
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            return Ok(true);
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                var response = await _httpService.GetAsync<RapidApiTeams>(
                    "https://api-nba-v1.p.rapidapi.com/teams/league/standard",
                    new Dictionary<string, string>
                    {
                        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
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
                result = GenericResponse.CreateErrorResponse(ex);
                result.Data = messages;
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("GetPlayers")]
        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            return Ok(true);
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                var teams = await _persistService.FindMany<NbaTeam>(_ => _.Id != "");
                var teamIds = teams.Select(_ => _.Id).ToList();
                var players = new List<NbaPlayer>();

                foreach (var teamId in teamIds)
                {
                    var response = await _httpService.GetAsync<RapidApiPlayers>(
                        $"https://api-nba-v1.p.rapidapi.com/players/teamId/{teamId}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    var teamPlayers = response?.Api?.players?.Where(_ =>
                        _.leagues?.standard?.active == "1" && _.yearsPro != "0" && _.startNba != "0").ToList();

                    if (teamPlayers == null)
                    {
                        continue;
                    }

                    foreach (var player in teamPlayers)
                    {
                        players.Add(new NbaPlayer
                        {
                            Id = player.playerId,
                            FirstName = player.firstName,
                            Jersey = player.leagues.standard.jersey,
                            LastName = player.lastName,
                            Position = player.leagues.standard.pos,
                            Season = "20-21",
                            TeamId = player.teamId
                        });
                    }
                }

                var retVal = await _persistService.BatchSave(players);

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                result.Data = messages;
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("CreateContest")]
        [HttpGet]
        public async Task<IActionResult> CreateContest()
        {
            var messages = new List<string>();
            
            ITransaction tx = null;
            
            try
            {
                var gameDates = new[]
                {
                    DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)).ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
                };

                var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                var dayOfGamesToGet = today.ToString("yyyy-MM-dd");

                var games = new List<Models.Games.Game>();

                foreach (var gameDate in gameDates)
                {
                    var response = await _httpService.GetAsync<RapidApiGames>(
                        $"https://api-nba-v1.p.rapidapi.com/games/date/{gameDate}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });
                    games.AddRange(response.Api.games.Where(_ => ToStringDayDate(_.startTimeUTC) == dayOfGamesToGet));
                }

                if (!games.Any())
                {
                    return Ok($"No Game Found for {dayOfGamesToGet}");
                }

                var contest = new Contest
                {
                    GameDate = dayOfGamesToGet,
                    Sport = "Basketball"
                };

                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.SaveAsync(contest);

                var dbGames = new List<Game>();
                foreach (var game in games)
                {
                    dbGames.Add(new Game
                    {
                        ContestId = contest.Id,
                        HTeamId = game.hTeam.teamId,
                        VTeamId = game.vTeam.teamId,
                        Id = game.gameId,
                        StartTime = TimeZoneInfo.ConvertTimeToUtc(game.startTimeUTC)
                    });
                }

                var sqlInsert =
                    "INSERT INTO `game` (`id`,`contestId`,`hTeamId`,`vTeamId`,`startTime`) VALUES ";
                var sqlValues =
                    string.Join(",", dbGames.Select(_ => $"('{_.Id}', {_.ContestId}, '{_.HTeamId}', '{_.VTeamId}', convert_tz('{_.StartTime:u}', '+00:00', '+00:00'))"));

                await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues};").ExecuteUpdateAsync();

                var hPlayers = new List<NbaPlayer>();
                var vPlayers = new List<NbaPlayer>();

                foreach (var dbGame in dbGames)
                {
                    hPlayers.AddRange(await _persistService.FindMany<NbaPlayer>(_ => _.TeamId == dbGame.HTeamId));
                    vPlayers.AddRange(await _persistService.FindMany<NbaPlayer>(_ => _.TeamId == dbGame.VTeamId));
                }

                //get home players
                var gamePlayers = hPlayers.Select(hPlayer => new GamePlayer
                    {
                        ContestId = contest.Id,
                        GameId = dbGames.First(_ => _.HTeamId == hPlayer.TeamId && _.ContestId == contest.Id).Id,
                        TeamId = hPlayer.TeamId,
                        PlayerId = hPlayer.Id,
                        TeamLocation = LocationType.Home
                    })
                    .ToList();

                //get visitor players
                gamePlayers.AddRange(
                    vPlayers.Select(vPlayer => new GamePlayer
                        {
                            ContestId = contest.Id,
                            GameId = dbGames.First(_ => _.VTeamId == vPlayer.TeamId && _.ContestId == contest.Id).Id, 
                            TeamId = vPlayer.TeamId,
                            PlayerId = vPlayer.Id,
                            TeamLocation = LocationType.Visitor
                        })
                        .ToList()
                );


                sqlInsert =
                    "INSERT INTO `gamePlayer` (`contestId`,`GameId`,`teamId`,`teamLocation`,`playerId`) VALUES ";
                sqlValues =
                    string.Join(",",gamePlayers.Select(_ => $"({_.ContestId},'{_.GameId}','{_.TeamId}','{_.TeamLocation}','{_.PlayerId}')"));

                await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues};").ExecuteUpdateAsync();

                await tx.CommitAsync();
                
                return Ok(true);
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                var result = GenericResponse.CreateErrorResponse(ex);
                result.Data = messages;
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        private string ToStringDayDate(DateTime dateNoTimeZone)
        {
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(dateNoTimeZone);

            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(utcDate, easternZone);

            return today.ToString("yyyy-MM-dd");
        }
    }
}
