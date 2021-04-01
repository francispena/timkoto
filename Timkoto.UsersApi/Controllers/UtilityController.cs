using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/utility/v1")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IHttpService _httpService;

        private readonly IPersistService _persistService;

        private readonly IRapidNbaStatistics _rapidNbaStatistics;

        private readonly IContestService _contestService;

        private readonly ITransactionService _transactionService;

        private readonly ICognitoUserStore _cognitoUserStore;

        public UtilityController(IHttpService httpService, IPersistService persistService,
            IRapidNbaStatistics rapidNbaStatistics, IContestService contestService,
            ITransactionService transactionService, ICognitoUserStore cognitoUserStore)
        {
            _httpService = httpService;
            _persistService = persistService;
            _rapidNbaStatistics = rapidNbaStatistics;
            _contestService = contestService;
            _transactionService = transactionService;
            _cognitoUserStore = cognitoUserStore;
        }

        [Route("CheckHealth")]
        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            var messages = new List<string> { "HealthController.CheckHealth" };

            try
            {
                var authenticateResult = await _cognitoUserStore.AuthenticateAsync("iskoap@yahoo.com", "password1", messages);

                var dbTestResult = await _persistService.FindOne<User>(_ => _.Email == "iskoap@yahoo.com");
                var result = new
                {
                    CognitoTest = authenticateResult.Result,
                    DBTest = dbTestResult
                };

                if (authenticateResult.IsSuccess && dbTestResult != null)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(403, result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
               // _lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }
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
            //return Ok(true);
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

                    var teamPlayers = response?.Api?.players?.Where(_ => _.leagues?.standard?.active == "1").ToList();

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

                var sqlInsert = "INSERT INTO `timkotodb`.`nbaPlayer` (`id`, `teamId`, `firstName`, `lastName`, `jersey`, `position`, `season`, `salary`) VALUES ";
                var sqlValues = string.Join(",", players.Select(_ => $"('{_.Id}', '{_.TeamId}', '{_.FirstName.Replace("'", "''")}', '{_.LastName.Replace("'", "''")}', '{_.Jersey}', '{_.Position.Replace("'", "''")}', '2020', '0.00')"));

                var retVal = await _persistService.ExecuteSql($"{sqlInsert}{sqlValues}");

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

                var contestToCheck = await _persistService.FindOne<Contest>(_ => _.GameDate == dayOfGamesToGet);

                if (contestToCheck != null)
                {
                    dayOfGamesToGet = today.AddDays(1).ToString("yyyy-MM-dd");
                    contestToCheck = await _persistService.FindOne<Contest>(_ => _.GameDate == dayOfGamesToGet);
                }

                if (contestToCheck != null)
                {
                    return StatusCode(403, $"Contest for the day {dayOfGamesToGet} exists.");
                }

                var gameDates = new[]
                {
                    today.ToUniversalTime().AddDays(-1).ToString("yyyy-MM-dd"),
                    today.ToUniversalTime().ToString("yyyy-MM-dd"),
                    today.ToUniversalTime().AddDays(1).ToString("yyyy-MM-dd"),
                    today.ToUniversalTime().AddDays(2).ToString("yyyy-MM-dd")
                };

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
                    ContestState = ContestState.Upcoming,
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
                    TeamLocation = LocationType.Home,
                    Salary = hPlayer.Salary
                }).ToList();

                //get visitor players
                gamePlayers.AddRange(
                    vPlayers.Select(vPlayer => new GamePlayer
                    {
                        ContestId = contest.Id,
                        GameId = dbGames.First(_ => _.VTeamId == vPlayer.TeamId && _.ContestId == contest.Id).Id,
                        TeamId = vPlayer.TeamId,
                        PlayerId = vPlayer.Id,
                        TeamLocation = LocationType.Visitor,
                        Salary = vPlayer.Salary
                    }).ToList()
                );

                sqlInsert =
                    "INSERT INTO `gamePlayer` (`contestId`,`GameId`,`teamId`,`teamLocation`,`playerId`,`salary`) VALUES ";
                sqlValues =
                    string.Join(",", gamePlayers.Select(_ => $"({_.ContestId},'{_.GameId}','{_.TeamId}','{_.TeamLocation}','{_.PlayerId}','{_.Salary}')"));

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
            var setPrizesInTransactionResult = await _contestService.SetPrizesInTransaction(new List<string>());

            var createContestResult = await CreateContest();

            return Ok(new
            {
                setPrizesInTransactionResult,
                createContestResult
            });
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
                var utcDate = DateTime.UtcNow;

                var gameDates = new[]
                {
                    utcDate.AddDays(-1).ToString("yyyy-MM-dd"),
                    utcDate.ToString("yyyy-MM-dd"),
                    utcDate.AddDays(1).ToString("yyyy-MM-dd")
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

                var today = TimeZoneInfo.ConvertTimeFromUtc(utcDate, easternZone);
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
                    return StatusCode(403, $"No Game Found for {dayOfGamesToGet}");
                }

                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState != ContestState.Finished);

                if (contest == null)
                {
                    return StatusCode(403, "No contest Found");
                }

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
                    string.Join(";\r\n", dbGames.Select(_ => $"UPDATE `timkotodb`.`game` SET `id` = '{_.Id}' WHERE ((`hTeamId` = '{_.HTeamId}' or `vTeamId` = '{_.VTeamId}') and `contestId` = '{_.ContestId}')"));

                var sqlUpdateGamePlayer = string.Join(";\r\n", dbGames.Select(_ => $"UPDATE `timkotodb`.`gamePlayer` SET `GameId` = '{_.Id}' WHERE (`contestId` = '{_.ContestId}' and `teamId` in ('{_.VTeamId}', '{_.HTeamId}'))"));

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

        [Route("TestService/{start}/{count}")]
        [HttpGet]
        public async Task<IActionResult> TestService([FromRoute] int start, [FromRoute] int count)
        {
 
            //return Ok();
            var contestId = 2;
            var messages = new List<string>();
            var players = await _persistService.FindMany<User>(_ => _.OperatorId == 10010 && _.UserType == UserType.Player);

            var sqlQuery =
                $@"SELECT np.id as playerId, concat(lastName, ', ', firstName) as playerName, np.jersey, nt.nickName as team, np.position, gp.salary FROM timkotodb.gamePlayer gp 
                        inner join nbaPlayer np
                        on gp.playerId = np.id
                        inner join nbaTeam nt 
                        on nt.id = np.teamId 
                        where np.season = '2020' and gp.contestId = '{contestId}';";

            var contestPlayers = await _persistService.SqlQuery<ContestPlayer>(sqlQuery);

            var groupedPlayers = contestPlayers.GroupBy(_ => _.Position).Select(g => new { Position = g.Key, Players = g.ToList() }).ToList();

            foreach (var player in players.OrderBy(_ => _.Id).Skip(start).Take(count))
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
                    var playerCount = groupedPlayer.Players.Count;
                    var index1 = random.Next(0, playerCount - 1);

                    if (groupedPlayer.Position != "C")
                    {
                        int index2;
                        do
                        {
                            index2 = random.Next(0, playerCount - 1);
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
                        ContestId = contestId,
                        PlayerTeamId = 0,
                        TeamName = player.UserName
                    }
                }, messages);
            }

            return Ok("true");
        }
    }
}
