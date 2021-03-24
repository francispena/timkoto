using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/utility/v1/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IHttpService _httpService;

        private readonly IPersistService _persistService;

        private readonly IRapidNbaStatistics _rapidNbaStatistics;

        private readonly IContestService _contestService;

        public UtilityController(IHttpService httpService, IPersistService persistService, IRapidNbaStatistics rapidNbaStatistics, IContestService contestService)
        {
            _httpService = httpService;
            _persistService = persistService;
            _rapidNbaStatistics = rapidNbaStatistics;
            _contestService = contestService;
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

                Console.WriteLine(JsonConvert.SerializeObject(teamIds));

                foreach (var teamId in teamIds)
                {
                    
                    Console.WriteLine(teamId);

                    var response = await _httpService.GetAsync<RapidApiPlayers>(
                        $"https://api-nba-v1.p.rapidapi.com/players/teamId/{teamId}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    await Task.Delay(1000);

                    var teamPlayers = response?.Api?.players?.Where(_ =>
                        _.leagues?.standard?.active == "1" && _.yearsPro != "0" && _.startNba != "0").ToList();

                    if (teamPlayers == null)
                    {
                        continue;
                    }

                    foreach (var player in teamPlayers)
                    {
                        if (player.leagues.standard.pos.Contains("C"))
                        {
                            player.leagues.standard.pos = "C";
                        }
                        else if (player.leagues.standard.pos == "G")
                        {
                            player.leagues.standard.pos = "PG";
                        }
                        else if (player.leagues.standard.pos == "F")
                        {
                            player.leagues.standard.pos = "PF";
                        }
                        else if (player.leagues.standard.pos == "G-F")
                        {
                            player.leagues.standard.pos = "SF";
                        }
                        else if (player.leagues.standard.pos == "F-G")
                        {
                            player.leagues.standard.pos = "SG";
                        }
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

                var contestToCheck = _persistService.FindOne<Contest>(_ => _.GameDate == dayOfGamesToGet);

                if (contestToCheck != null)
                {
                    return StatusCode(403, "Contest for the exists.");
                }

                var games = new List<RapidApiGamesGame>();

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
                    Sport = "Basketball",
                    ContestState = ContestState.Scheduled,
                    SalaryCap = 60000
                };

                var operators =
                    await _persistService.FindMany<User>(_ => _.IsActive && _.UserType == UserType.Operator);

                var dbSession = _persistService.GetSession();
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
                        StartTime = TimeZoneInfo.ConvertTimeToUtc(game.startTimeUTC),
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

                foreach (var @operator in operators)
                {
                    var contestPool = new ContestPool
                    {
                        ContestId = contest.Id,
                        OperatorId = @operator.Id,
                        ContestPrizeId = 1
                    };

                    await dbSession.SaveAsync(contestPool);
                }

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

        [Route("GetLiveStats")]
        [HttpGet]
        public async Task<IActionResult> GetLiveStats()
        {
            var result = await _rapidNbaStatistics.GetLiveStats(new List<string>());

            return Ok(result);
        }

        [Route("GetFinalStats")]
        [HttpGet]
        public async Task<IActionResult> GetFinalStats()
        {
            var result = await _rapidNbaStatistics.GetFinalStats(new List<string>());

            return Ok(result);
        }

        [Route("RankTeams")]
        [HttpPost]
        public async Task<IActionResult> RankTeams()
        {
            var result = await _contestService.RankTeams(new List<string>());

            return Ok(result);
        }

        [Route("RankAndSetPrizes")]
        [HttpPost]
        public async Task<IActionResult> RankAndSetPrizes()
        {
            var result = await _contestService.RankAndSetPrizes(new List<string>());

            return Ok(result);
        }


        [Route("BroadcastRanks")]
        [HttpPost]
        public async Task<IActionResult> BroadcastRanks()
        {
            await _contestService.BroadcastRanks(new List<string>());

            return Ok();
        }

        [Route("TestService")]
        [HttpGet]
        public async Task<IActionResult> TestService()
        {
            var emailService = new EmailService();
            var result  = await emailService.SendRegistrationLink("francisgail.pena@yahoo.com", "https://timkoto.com/register/5623867386723675396", new List<string>());

            return Ok(result);
        }
    }
}
