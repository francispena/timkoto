using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<bool> GetLiveStats(List<string> messages)
        {
            try
            {
                var responseLive = await _httpService.GetAsync<RapidApiLive>(
                    $"https://api-nba-v1.p.rapidapi.com/games/live/",
                    new Dictionary<string, string>
                    {
                        {"x-rapidapi-key", "052d7c2822msh1effd682c0dbce0p113fabjsn219fbe03967c"},
                        {"x-rapidapi-host", "api-nba-v1.p.rapidapi.com"}
                    });

                if (responseLive?.api?.games == null)
                {
                    return false;
                }

                var gameIds = responseLive.api.games.Select(_ => _.gameId).ToList();
                var updateResult = false;

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
                    var retVal = await _persistService.ExecuteSql($"{sqlUpdate};");
                }

                await GetStatsForFinishedGames(messages);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> GetFinalStats(List<string> messages)
        {
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return false;
                }

                var games = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id);
                var gameIds = games.Select(_ => _.Id);

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
                    var updateResult = await _persistService.ExecuteSql($"{sqlUpdate};");

                    return updateResult;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        private async Task GetStatsForFinishedGames(List<string> messages)
        {
            ITransaction tx = null;

            try
            {
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return;
                }

                var unFinishedGames = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id && _.Finished == false);
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
            }
            catch
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }
            }
        }
    }
}
