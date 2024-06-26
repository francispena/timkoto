﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Timkoto.Data.Enumerations;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class RapidNbaStatistics : IRapidNbaStatistics
    {
        private readonly IPersistService _persistService;

        private readonly IHttpService _httpService;

        public RapidNbaStatistics(IPersistService persistService, IHttpService httpService)
        {
            _persistService = persistService;
            _httpService = httpService;
        }

        public async Task<string> GetLiveStats(List<string> messages)
        {
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ =>
                                  _.ContestState == ContestState.Ongoing || _.ContestState == ContestState.Upcoming);

                if (contest == null)
                {
                    return "No ongoing contest";
                }

                var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false && _.StartTime < DateTime.UtcNow);

                if (!games.Any())
                {
                    return "No game schedule";
                }

                if (contest.ContestState == ContestState.Upcoming)
                {
                    contest.ContestState = ContestState.Ongoing;
                    var updateContestResult = await _persistService.Update(contest);
                    if (!updateContestResult)
                    {
                        return "Update contest to Ongoing failed.";
                    }
                }

                var gameIds = games.Select(_ => _.Id).ToList();
                var teamPlayerIds = new List<TeamPlayerId>();

                foreach (var game in games)
                {
                    var gameId = game.Id;

                    var response = await _httpService.GetAsync<GamePlayerStatiscs>(
                    $"https://api-nba-v1.p.rapidapi.com/statistics/players/gameId/{gameId}",
                    new Dictionary<string, string>
                    {
                        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                    });

                    if (response?.api?.statistics == null)
                    {
                        continue;
                    }

                    var updates = new List<string>();

                    foreach (var apiStatistic in response.api.statistics)
                    {

                        decimal.TryParse(apiStatistic.points, out decimal points);
                        decimal.TryParse(apiStatistic.totReb, out decimal totReb);
                        totReb *= 1.2m;
                        decimal.TryParse(apiStatistic.assists, out decimal assists);
                        assists *= 1.5m;
                        decimal.TryParse(apiStatistic.steals, out decimal steals);
                        steals *= 2;
                        decimal.TryParse(apiStatistic.blocks, out decimal blocks);
                        blocks *= 2;
                        decimal.TryParse(apiStatistic.turnovers, out decimal turnovers);
                        turnovers *= -1;
                        var totalPoints = points + totReb + assists + steals + blocks + turnovers;

                        updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`contestId` = '{contest.Id}' and playerId = '{apiStatistic.playerId}')");
                    }

                    if (!updates.Any())
                    {
                        continue;
                    }

                    var sqlUpdate = string.Join(";", updates);
                    var updateResult = await _persistService.ExecuteSql($"{sqlUpdate};");

                    if (!updateResult)
                    {
                        continue;
                    }

                    var gameDetails = await _httpService.GetAsync<RapidApiGames>(
                        $"https://api-nba-v1.p.rapidapi.com/games/gameId/{gameId}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    if (gameDetails?.Api?.games == null || !gameDetails.Api.games.Any())
                    {
                        continue;
                    }
                    
                    if (string.Equals(gameDetails.Api?.games[0]?.statusGame, "Finished", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await _persistService.ExecuteSql($"UPDATE `timkotodb`.`game` SET `finished` = '1' WHERE (`id` = '{gameId}');");
                    }
                }

                return "Stats updated.";
            }
            catch (Exception ex)
            {
                return $"GetLiveStats error - {ex.Message}";
            }
        }

        public async Task<string> GetLiveStats2(List<string> messages)
        {
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ =>
                                   _.ContestState == ContestState.Ongoing || _.ContestState == ContestState.Upcoming);

                if (contest == null)
                {
                    return "No ongoing contest";
                }

                //var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false);
                //if (!games.Any())
                //{
                //    return "No game schedule";
                //}

                //var firstGameTime = games.OrderBy(_ => _.StartTime).First();

                //if (contest.ContestState == ContestState.Upcoming && firstGameTime.StartTime.Subtract(DateTime.UtcNow).TotalHours < 1)
                //{
                //    await UpdateGameIds(messages);
                //}

                var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false && _.StartTime < DateTime.UtcNow);

                if (!games.Any())
                {
                    return "No game schedule";
                }

                await UpdateGameIds(messages);

                games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false && _.StartTime < DateTime.UtcNow);

                if (!games.Any())
                {
                    return "No game schedule";
                }

                if (contest.ContestState == ContestState.Upcoming)
                {
                    contest.ContestState = ContestState.Ongoing;
                    var updateContestResult = await _persistService.Update(contest);
                    if (!updateContestResult)
                    {
                        return "Update contest to Ongoing failed.";
                    }
                }

                var gameIds = games.Select(_ => _.Id).ToList();
                var teamPlayerIds = new List<TeamPlayerId>();

                foreach (var gameId in gameIds)
                {
                    var response = await _httpService.GetAsync<GamePlayerStatiscs>(
                    $"https://api-nba-v1.p.rapidapi.com/statistics/players/gameId/{gameId}",
                    new Dictionary<string, string>
                    {
                        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                    });

                    if (response?.api?.statistics == null || !response.api.statistics.Any())
                    {
                        continue;
                    }

                    var updates = new List<string>();

                    foreach (var apiStatistic in response.api.statistics)
                    {
                        decimal.TryParse(apiStatistic.points, out decimal points);
                        decimal.TryParse(apiStatistic.totReb, out decimal totReb);
                        totReb *= 1.2m;
                        decimal.TryParse(apiStatistic.assists, out decimal assists);
                        assists *= 1.5m;
                        decimal.TryParse(apiStatistic.steals, out decimal steals);
                        steals *= 2;
                        decimal.TryParse(apiStatistic.blocks, out decimal blocks);
                        blocks *= 2;
                        decimal.TryParse(apiStatistic.turnovers, out decimal turnovers);
                        turnovers *= -1;
                        var totalPoints = points + totReb + assists + steals + blocks + turnovers;

                        updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`contestId` = '{contest.Id}' and playerId = '{apiStatistic.playerId}')");
                    }

                    var sqlUpdate = string.Join(";", updates);
                    await _persistService.ExecuteSql($"{sqlUpdate};");

                    teamPlayerIds.AddRange(response.api.statistics.Where(_ => _.points != "0" || _.totReb != "0" || _.assists != "0" || _.steals != "0"
                                                                              || _.blocks != "0" || _.turnovers != "0").Select(_ => new TeamPlayerId
                    { PlayerId = _.playerId, TeamId = _.teamId }).ToList());

                    var gameDetails = await _httpService.GetAsync<RapidApiGames>($"https://api-nba-v1.p.rapidapi.com/games/gameId/{gameId}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    if (gameDetails?.Api?.games == null || !gameDetails.Api.games.Any())
                    {
                        continue;
                    }

                    if (string.Equals(gameDetails.Api?.games[0]?.statusGame, "Finished", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await _persistService.ExecuteSql($"UPDATE `timkotodb`.`game` SET `finished` = '1' WHERE (`id` = '{gameId}');");
                    }
                }

                var dbSession = _persistService.GetSession();
                
                var createTempTableSql =
                    "CREATE TEMPORARY TABLE `timkotodb`.`tempTeamPlayerId` (`id` INT NOT NULL AUTO_INCREMENT, `teamId` VARCHAR(40) NULL, `playerId` VARCHAR(40) NULL,PRIMARY KEY(`id`));";

                await dbSession.CreateSQLQuery(createTempTableSql).ExecuteUpdateAsync();
                
                var sqlInsert = "INSERT INTO `timkotodb`.`tempTeamPlayerId` (`teamId`, `playerId`) VALUES ";
                var sqlValues = string.Join(",", teamPlayerIds.Select(_ => $"('{_.TeamId}', '{_.PlayerId}')"));

                await dbSession.CreateSQLQuery($"{sqlInsert} {sqlValues};").ExecuteUpdateAsync();

                var findSql = @$"select teamId, playerId
                        from `timkotodb`.`tempTeamPlayerId` t1
                    where not exists(
                        select 1
                    from `timkotodb`.`gamePlayer` t2
                        where t1.playerId = t2.playerId and contestId = {contest.Id}
                        );";

                var missingTeamPlayerIds = (await dbSession.CreateSQLQuery(findSql)
                    .SetResultTransformer(Transformers.AliasToBean<TeamPlayerId>()).ListAsync<TeamPlayerId>()).ToList();

                dbSession.Close();
                dbSession.Dispose();

                if (missingTeamPlayerIds.Any())
                {
                    await FixMissingPlayers(missingTeamPlayerIds, contest.Id, messages);
                }

                return $"Unmatched PlayerIds - {JsonConvert.SerializeObject(missingTeamPlayerIds)}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> FixMissingPlayers(List<TeamPlayerId> teamPlayerIds, long contestId, List<string> messages)
        {
            try
            {
                foreach (var teamPlayerId in teamPlayerIds)
                {
                    var response = await _httpService.GetAsync<RapidApiPlayers>(
                        $"https://api-nba-v1.p.rapidapi.com/players/playerId/{teamPlayerId.PlayerId}",
                        new Dictionary<string, string>
                        {
                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    if (response?.Api?.players == null)
                    {
                        continue;
                    }

                    if (!response.Api.players.Any())
                    {
                        continue;
                    }

                    var player = response.Api.players[0];

                    var nbaPlayer = await _persistService.FindOne<NbaPlayer>(_ =>
                        _.FirstName == player.firstName && _.LastName == player.lastName);

                    if (nbaPlayer == null)
                    {
                        var sqlQuery =
                            $@"SELECT position, fname, lname, salary, team FROM timkotodb.fdPlayers where fname = '{player.firstName}' and lname = '{player.lastName}';";

                        var fdPlayers = await _persistService.SqlQuery<ContestPlayer>(sqlQuery);

                        var salary = 0m;
                        string position = null;
                        if (fdPlayers != null && fdPlayers.Any())
                        {
                            salary = fdPlayers[0].Salary;
                            position = fdPlayers[0].Position;
                        }

                        nbaPlayer = new NbaPlayer
                        {
                            FirstName = player.firstName,
                            Id = player.playerId,
                            LastName = player.lastName,
                            Jersey = player.leagues?.standard?.jersey,
                            Position = position ?? "XX",
                            Salary = salary,
                            Season = "2020",
                            TeamId = teamPlayerId.TeamId
                        };

                        await _persistService.Save(nbaPlayer);
                    }
                    else
                    {
                        await _persistService.ExecuteSql(
                            $"UPDATE `timkotodb`.`nbaPlayer` SET `id` = '{player.playerId}', `teamId` = '{teamPlayerId.TeamId}' WHERE (`id` = '{nbaPlayer.Id}');");
                    }

                    var gamePlayer = await _persistService.FindOne<GamePlayer>(_ => _.ContestId == contestId && _.TeamId == teamPlayerId.TeamId);

                    if (gamePlayer == null)
                    {
                        continue;
                    }

                    var newGamePlayer = new GamePlayer
                    {
                        ContestId = contestId,
                        GameId = gamePlayer.GameId,
                        TeamId = teamPlayerId.TeamId,
                        TeamLocation = gamePlayer.TeamLocation,
                        PlayerId = teamPlayerId.PlayerId
                    };

                    await _persistService.Save(newGamePlayer);
                }

                return "success";
            }
            catch (Exception ex)
            {
                return $"GetStatsForFinishedGames error - {ex.Message}";
            }
        }

        private async Task<string> GetStatsForFinishedGames(List<string> messages)
        {
            ITransaction tx = null;

            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return "No Contest Found";
                }

                var unFinishedGames = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false);

                if (!unFinishedGames.Any())
                {
                    return "Games Done";
                }

                var gameIds = unFinishedGames.Select(_ => _.Id);

                foreach (var gameId in gameIds)
                {
                    var response = await _httpService.GetAsync<RapidApiGames>(
                        $"https://api-nba-v1.p.rapidapi.com/games/gameId/{gameId}",
                        new Dictionary<string, string>
                        {
                        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                        });

                    if (response?.Api?.games == null || !response.Api.games.Any())
                    {
                        continue;
                    }

                    if (response.Api?.games[0]?.statusGame != "Finished")
                    {
                        continue;
                    }

                    var gamePlayerStatiscsResponse = await _httpService.GetAsync<GamePlayerStatiscs>(
                         $"https://api-nba-v1.p.rapidapi.com/statistics/players/gameId/{gameId}",
                         new Dictionary<string, string>
                         {
                                            {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                                            {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                         });

                    if (gamePlayerStatiscsResponse?.api?.statistics == null)
                    {
                        continue;
                    }

                    var updates = new List<string>();

                    foreach (var apiStatistic in gamePlayerStatiscsResponse.api.statistics)
                    {

                        decimal.TryParse(apiStatistic.points, out decimal points);
                        decimal.TryParse(apiStatistic.totReb, out decimal totReb);
                        totReb *= 1.2m;
                        decimal.TryParse(apiStatistic.assists, out decimal assists);
                        assists *= 1.5m;
                        decimal.TryParse(apiStatistic.steals, out decimal steals);
                        steals *= 2;
                        decimal.TryParse(apiStatistic.blocks, out decimal blocks);
                        blocks *= 2;
                        decimal.TryParse(apiStatistic.turnovers, out decimal turnovers);
                        turnovers *= -1;
                        var totalPoints = points + totReb + assists + steals + blocks + turnovers;

                        updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`gameId` = '{gameId}' and playerId = '{apiStatistic.playerId}')");
                    }

                    var sqlUpdate = string.Join(";", updates);

                    var dbSession = _persistService.GetSession();
                    tx = dbSession.BeginTransaction();

                    await dbSession.CreateSQLQuery(sqlUpdate).ExecuteUpdateAsync();
                    await dbSession.CreateSQLQuery($"UPDATE `timkotodb`.`game` SET `finished` = '1' WHERE (`id` = '{gameId}');").ExecuteUpdateAsync();

                    await tx.CommitAsync();

                }

                return "success";
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                return $"GetStatsForFinishedGames error - {ex.Message}";
            }
        }

        public async Task<string> UpdateGameIds(List<string> messages)
        {
            ITransaction tx = null;
            var dbSession = _persistService.GetSession();

            try
            {
                var utcDate = DateTime.UtcNow;

                var gameDates = new[]
                {
                    utcDate.AddDays(-2).ToString("yyyy-MM-dd"),
                    utcDate.AddDays(-1).ToString("yyyy-MM-dd"),
                    utcDate.ToString("yyyy-MM-dd"),
                    utcDate.AddDays(1).ToString("yyyy-MM-dd"),
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
                    return "TimeZoneLookUpError";
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
                    return  $"No Game Found for {dayOfGamesToGet}";
                }

                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState != ContestState.Finished);

                if (contest == null)
                {
                    return  "No contest Found";
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
                    string.Join(";\r\n", dbGames.Select(_ => $"UPDATE `timkotodb`.`game` SET `id` = '{_.Id}' WHERE (`hTeamId` = '{_.HTeamId}' and `vTeamId` = '{_.VTeamId}' and `contestId` = '{_.ContestId}')"));

                var sqlUpdateHomeGamePlayer = string.Join(";\r\n", dbGames.Select(_ => $"UPDATE `timkotodb`.`gamePlayer` SET `GameId` = '{_.Id}' WHERE (`contestId` = '{_.ContestId}' and `teamId` = '{_.VTeamId}' and teamLocation = 'Visitor')"));
                var sqlUpdateVisitorGamePlayer = string.Join(";\r\n", dbGames.Select(_ => $"UPDATE `timkotodb`.`gamePlayer` SET `GameId` = '{_.Id}' WHERE (`contestId` = '{_.ContestId}' and `teamId` = '{_.HTeamId}' and teamLocation = 'Home')"));

                tx = dbSession.BeginTransaction();

                await dbSession.CreateSQLQuery(sqlUpdateGame + ";").ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdateHomeGamePlayer + ";").ExecuteUpdateAsync();
                await dbSession.CreateSQLQuery(sqlUpdateVisitorGamePlayer + ";").ExecuteUpdateAsync();

                await tx.CommitAsync();
                dbSession.Close();
                dbSession.Dispose();

                return "Ok";
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                    dbSession.Close();
                    dbSession.Dispose();
                }

                return ex.Message;
            }
            finally
            {
                
            }
        }

        private string ToStringDayDate(DateTime dateNoTimeZone, TimeZoneInfo easternZone)
        {
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(dateNoTimeZone);

            var today = TimeZoneInfo.ConvertTimeFromUtc(utcDate, easternZone);

            return today.ToString("yyyy-MM-dd");
        }
    }
}
