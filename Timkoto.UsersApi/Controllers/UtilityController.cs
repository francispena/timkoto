using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/utility/v1/")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IHttpService _httpService;

        private readonly IPersistService _persistService;

        private readonly IRapidNbaStatistics _rapidNbaStatistics;

        private readonly IContestService _contestService;

        private readonly ITransactionService _transactionService;

        public UtilityController(IHttpService httpService, IPersistService persistService, IRapidNbaStatistics rapidNbaStatistics, IContestService contestService, ITransactionService transactionService)
        {
            _httpService = httpService;
            _persistService = persistService;
            _rapidNbaStatistics = rapidNbaStatistics;
            _contestService = contestService;
            _transactionService = transactionService;
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
                            Season = "2020",
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
                    DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
                };

                TimeZoneInfo easternZone = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                }

                if (easternZone == null)
                {
                    var genericResponse = GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.TimeZoneLookUpError);
                    return StatusCode(403, genericResponse);
                }

                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                var dayOfGamesToGet = today.ToString("yyyy-MM-dd");

                var contestToCheck = await _persistService.FindOne<Contest>(_ => _.GameDate == dayOfGamesToGet);

                if (contestToCheck != null)
                {
                    return StatusCode(403, $"Contest for the day {dayOfGamesToGet} exists.");
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
                    games.AddRange(response.Api.games.Where(_ => ToStringDayDate(_.startTimeUTC, easternZone) == dayOfGamesToGet));
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
                        Finished = false
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
                    string.Join(",", gamePlayers.Select(_ => $"({_.ContestId},'{_.GameId}','{_.TeamId}','{_.TeamLocation}','{_.PlayerId}')"));

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

        private string ToStringDayDate(DateTime dateNoTimeZone, TimeZoneInfo easternZone)
        {
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(dateNoTimeZone);

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
        [HttpGet]
        public async Task<IActionResult> RankTeams()
        {
            var result = await _contestService.RankTeams(new List<string>());

            return Ok(result);
        }

        [Route("SetPrizes")]
        [HttpGet]
        public async Task<IActionResult> SetPrizes()
        {
            var result = await _contestService.SetPrizes(new List<string>());

            return Ok(result);
        }

        [Route("SetPrizesInTransaction")]
        [HttpGet]
        public async Task<IActionResult> SetPrizesInTransaction()
        {
            var result = await _contestService.SetPrizesInTransaction(new List<string>());

            return Ok(result);
        }

        [Route("UpdateGameIds")]
        [HttpGet]
        public async Task<IActionResult> UpdateGameIds()
        {
            var messages = new List<string>();

            ITransaction tx = null;
            var dbSession = _persistService.GetSession();

            try
            {
                var gameDates = new[]
                {
                    DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
                };

                TimeZoneInfo easternZone = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                }

                if (easternZone == null)
                {
                    var genericResponse = GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.TimeZoneLookUpError);
                    return StatusCode(403, genericResponse);
                }

                var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
                var dayOfGamesToGet = today.ToString("yyyy-MM-dd");

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
                    games.AddRange(response.Api.games.Where(_ => ToStringDayDate(_.startTimeUTC, easternZone) == dayOfGamesToGet));
                }

                if (!games.Any())
                {
                    return Ok($"No Game Found for {dayOfGamesToGet}");
                }

                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
  
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
                        Finished = false
                    });
                }

                var sqlUpdateGame =
                    string.Join(";", dbGames.Select(_ => $"UPDATE `timkotodb`.`game` SET `id` = '{_.Id}' WHERE (`hTeamId` = '{_.HTeamId}' and `vTeamId` = '{_.VTeamId}' and `contestId` = '{_.ContestId}')"));

                var sqlUpdateGamePlayer =  string.Join(";", dbGames.Select(_ => $"UPDATE `timkotodb`.`gamePlayer` SET `GameId` = '{_.Id}' WHERE (`contestId` = '{_.ContestId}' and `teamId` in ('{_.VTeamId}', '{_.HTeamId}'))"));
                
                tx = dbSession.BeginTransaction();

                await dbSession.CreateSQLQuery(sqlUpdateGame + ";").ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdateGamePlayer + ";").ExecuteUpdateAsync();

                await tx.CommitAsync();
                dbSession.Close();
                dbSession.Dispose();

                return Ok(true);
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                    dbSession.Close();
                    dbSession.Dispose();
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

        //[Route("BroadcastRanks")]
        //[HttpPost]
        //public async Task<IActionResult> BroadcastRanks()
        //{
        //    await _contestService.BroadcastRanks(new List<string>());

        //    return Ok();
        //}

        [Route("TestService")]
        [HttpGet]
        public async Task<IActionResult> TestService()
        {
            var messages = new List<string>();
            var players = await _persistService.FindMany<User>(_ => _.OperatorId == 10010 && _.UserType == UserType.Player);

            var sqlQuery =
                $@"SELECT np.id as playerId, concat(lastName, ', ', firstName) as playerName, np.jersey, nt.nickName as team, np.position, gp.salary FROM timkotodb.gamePlayer gp 
                        inner join nbaPlayer np
                        on gp.playerId = np.id
                        inner join nbaTeam nt 
                        on nt.id = np.teamId 
                        where np.season = '2020' and gp.contestId = '{7}';";

            var contestPlayers = await _persistService.SqlQuery<ContestPlayer>(sqlQuery);

            var groupedPlayers = contestPlayers.GroupBy(_ => _.Position).Select(g => new { Position = g.Key, Players = g.ToList() }).ToList();

            foreach (var player in players)
            {
                var transaction = await _transactionService.AddTransaction(new AddTransactionRequest
                {
                    AgentId = player.AgentId,
                    Amount = 100,
                    OperatorId = player.OperatorId,
                    TransactionType = TransactionType.WalletDebit,
                    UserId = player.Id,
                    UserType = UserType.Player
                }, true, messages);

                if (!transaction.IsSuccess)
                {
                    continue;
                }

                foreach (var contestPlayer in groupedPlayers.SelectMany(_ => _.Players))
                {
                    contestPlayer.Selected = false;
                }

                foreach (var groupedPlayer in groupedPlayers)
                {
                    var random = new Random();
                    var count = groupedPlayer.Players.Count;
                    var index1 = random.Next(0, count - 1);

                    if (groupedPlayer.Position != "C")
                    {
                        int index2;
                        do
                        {
                            index2 = random.Next(0, count - 1);
                        } while (index1 == index2);

                        groupedPlayer.Players[index1].Selected = true;
                        groupedPlayer.Players[index2].Selected = true;
                    }
                    else
                    {
                        groupedPlayer.Players[index1].Selected = true;
                    }
                }

                var playerLineUp = (List<PlayerLineUp>)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(groupedPlayers), typeof(List<PlayerLineUp>));

                await _contestService.SubmitLineUp(new LineUpRequest
                {
                    LineUp = playerLineUp,
                    LineUpTeam = new LineUpTeam
                    {
                        AgentId = player.AgentId,
                        OperatorId = player.OperatorId,
                        UserId = player.Id,
                        ContestId = 7,
                        PlayerTeamId = 0,
                        TeamName = player.UserName
                    }
                }, messages);
            }


            return Ok();
        }
    }
}
