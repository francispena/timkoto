using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHibernate;
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
                //var responseLive = await _httpService.GetAsync<RapidApiLive>(
                //    $"https://api-nba-v1.p.rapidapi.com/games/live/",
                //    new Dictionary<string, string>
                //    {
                //        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                //        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                //    });

                //if (responseLive?.api?.games == null)
                //{
                //    return "NoGame";
                //}

                //var gameIds = responseLive.api.games.Select(_ => _.gameId).ToList();

                var contest = await _persistService.FindOne<Contest>(_ =>
                    _.ContestState == ContestState.Ongoing || _.ContestState == ContestState.Upcoming);

                if (contest == null)
                {
                    return "No Ongoing or Upcoming contest.";
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

                var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id);

                if (!games.Any())
                {
                    return "No live results.";
                }

                foreach (var game in games)
                {
                    var startTime = TimeZoneInfo.ConvertTimeToUtc(game.StartTime);
                    if (DateTime.UtcNow.Subtract(startTime).TotalMinutes <= 0 || game.Finished)
                    {
                        continue;
                    }

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

                        updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`gameId` = '{gameId}' and playerId = '{apiStatistic.playerId}')");
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

        public async Task<string> GetFinalStats(List<string> messages)
        {
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return "No onging contest";
                }

                var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id);
                var gameIds = games.Select(_ => _.Id);
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

                        updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`gameId` = '{gameId}' and playerId = '{apiStatistic.playerId}')");
                    }

                    var sqlUpdate = string.Join(";", updates);
                    await _persistService.ExecuteSql($"{sqlUpdate};");

                    teamPlayerIds.AddRange(response.api.statistics.Select(_ => new TeamPlayerId
                    { PlayerId = _.playerId, TeamId = _.teamId }));
                }

                var createTempTableSql =
                    "CREATE TEMPORARY TABLE `timkotodb`.`tempTeamPlayerId` (`id` INT NOT NULL AUTO_INCREMENT, `teamId` VARCHAR(40) NULL, `playerId` VARCHAR(40) NULL,PRIMARY KEY(`id`));";

                var createResult = await _persistService.ExecuteSql(createTempTableSql);

                var sqlInsert = "INSERT INTO `timkotodb`.`tempTeamPlayerId` (`teamId`, `playerId`) VALUES ";
                var sqlValues = string.Join(",", teamPlayerIds.Select(_ => $"('{_.TeamId}', '{_.PlayerId}')"));

                var sqlInsertResult = await _persistService.ExecuteSql($"{sqlInsert} {sqlValues};");

                var findSql = @"select teamId, playerId
                        from `timkotodb`.`tempTeamPlayerId` t1
                    where not exists(
                        select 1
                    from `timkotodb`.`gamePlayer` t2
                        where t1.teamId = t2.teamId and t1.playerId = t2.playerId
                        );";

                var missingTeamPlayerIds = await _persistService.SqlQuery<TeamPlayerId>(findSql);


                return $"Unmatched PlayerIds - {JsonConvert.SerializeObject(missingTeamPlayerIds)}";
            }
            catch (Exception ex)
            {
                return ex.Message;
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
    }
}
