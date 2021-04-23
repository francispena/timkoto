using System;
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
    public class OfficialNbaStatistics : IOfficialNbaStatistics
    {
        private readonly IPersistService _persistService;

        private readonly IHttpService _httpService;

        public OfficialNbaStatistics(IPersistService persistService, IHttpService httpService)
        {
            _persistService = persistService;
            _httpService = httpService;
        }

        public async Task<string> GetLiveStats(List<string> messages)
        {
            ITransaction tx = null;
            try
            {
                var contest = await _persistService.FindOne<Contest>(_ =>
                    _.ContestState == ContestState.Ongoing || _.ContestState == ContestState.Upcoming);

                if (contest == null)
                {
                    return "No ongoing contest";
                }

                TimeZoneInfo easternZone = null;

                easternZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Eastern Standard Time" : "America/New_York");

                var estNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                var schedules =
                    await _persistService.FindMany<OfficialNbaSchedules>(_ => _.GameDate == contest.GameDate && _.GameDateTimeEst < estNow);

                if (!schedules.Any())
                {
                    return "No game schedule";
                }

                var sqlInsert =
                    "INSERT INTO `timkotodb`.`officialNbaPlayerStats` (`personId`, `teamId`, `teamName`, `location`, `firstName`, `familyName`, `points`, `assists`, `blocks`, `steals`, `reboundsTotal`, `turnovers`) VALUES ";
                var sqlValues = new List<string>();

                foreach (var schedule in schedules)
                {
                    var response = await _httpService.GetAsync<NbaApiPlayerStats>($"https://cdn.nba.com/static/json/liveData/boxscore/boxscore_{schedule.GameId}.json",
                        new Dictionary<string, string>
                        {
                        });

                    if (response?.game?.awayTeam?.players != null && response.game.awayTeam.players.Any())
                    {
                        foreach (var player in response.game.awayTeam.players)
                        {
                            if (player.statistics == null)
                            {
                                continue;
                            }

                            sqlValues.Add($"('{player.personId}', '{response.game.awayTeam.teamId}', '{response.game.awayTeam.teamName.Replace("'", "''")}', 'Visitor', '{player.firstName.Replace("'", "''")}', '{player.familyName.Replace("'", "''")}', '{player.statistics.points}', '{player.statistics.assists}', '{player.statistics.blocks}', '{player.statistics.steals}', '{player.statistics.reboundsTotal}', '{player.statistics.turnovers}')");
                        }
                    }

                    if (response?.game?.awayTeam?.players == null || !response.game.homeTeam.players.Any())
                    {
                        continue;
                    }

                    {
                        foreach (var player in response.game.homeTeam.players)
                        {
                            if (player.statistics == null)
                            {
                                continue;
                            }

                            sqlValues.Add($"('{player.personId}', '{response.game.homeTeam.teamId}', '{response.game.homeTeam.teamName.Replace("'", "''")}', 'Home', '{player.firstName.Replace("'", "''")}', '{player.familyName.Replace("'", "''")}', '{player.statistics.points}', '{player.statistics.assists}', '{player.statistics.blocks}', '{player.statistics.steals}', '{player.statistics.reboundsTotal}', '{player.statistics.turnovers}')");
                        }
                    }
                }

                if (sqlValues.Any())
                {
                    var truncateExecuteSql = await _persistService.ExecuteSql("TRUNCATE `timkotodb`.`officialNbaPlayerStats`;");

                    if (!truncateExecuteSql)
                    {
                        return "Truncate Failed";
                    }

                    var insertQuery = $"{sqlInsert}{string.Join(",", sqlValues)};";

                    var retVal = await _persistService.ExecuteSql(insertQuery);

                    if (!retVal)
                    {
                        return "Insert official Result - false";
                    }

                    var sqlQuery =
                        @"SELECT n.id as playerId, o.points, o.reboundsTotal, o.assists, o.steals, o.blocks, o.turnovers, n.teamId FROM timkotodb.officialNbaPlayerStats o 
                            inner join timkotodb.nbaPlayer n 
                            on n.lastName = o.familyName and n.firstName = o.firstName
                            inner join timkotodb.nbaTeam t 
                            on t.nickName = o.teamName and t.id = n.teamId;";

                    var officialPlayerStats = await _persistService.SqlQuery<OfficialPlayerStats>(sqlQuery);

                    if (officialPlayerStats.Any())
                    {
                        var updates = new List<string>();
                        var teamPlayerIds = new List<TeamPlayerId>();

                        foreach (var stats in officialPlayerStats)
                        {
                            decimal points = stats.Points;
                            decimal totReb = stats.ReboundsTotal;
                            totReb *= 1.2m;
                            decimal assists = stats.Assists;
                            assists *= 1.5m;

                            decimal steals = stats.Steals;
                            steals *= 2;

                            decimal blocks = stats.Blocks;
                            blocks *= 2;
                            decimal turnovers = stats.TurnOvers;
                            turnovers *= -1;

                            var totalPoints = points + totReb + assists + steals + blocks + turnovers;

                            updates.Add($@"UPDATE `timkotodb`.`gamePlayer` SET `points` = {points}, `rebounds` = {totReb}, `assists` = {assists}, `steals` = {steals}, `blocks` = {blocks}, `turnOvers` = {turnovers}, `totalPoints` = {totalPoints} WHERE(`contestId` = '{contest.Id}' and playerId = '{stats.PlayerId}')");
                        }

                        var sqlUpdate = string.Join(";", updates);
                        var updateResult = await _persistService.ExecuteSql($"{sqlUpdate};");

                        teamPlayerIds.AddRange(officialPlayerStats.Where(_ => _.Points != 0 || _.ReboundsTotal != 0 || _.Assists != 0 || _.Steals != 0
                                                                              || _.Blocks != 0 || _.TurnOvers != 0).Select(_ => new TeamPlayerId
                                                                              { PlayerId = _.PlayerId, TeamId = _.TeamId }).ToList());

                        var dbSession = _persistService.GetSession();

                        tx = dbSession.BeginTransaction();

                        var createTempTableSql =
                            "CREATE TEMPORARY TABLE `timkotodb`.`tempTeamPlayerId` (`id` INT NOT NULL AUTO_INCREMENT, `teamId` VARCHAR(40) NULL, `playerId` VARCHAR(40) NULL,PRIMARY KEY(`id`));";

                        await dbSession.CreateSQLQuery(createTempTableSql).ExecuteUpdateAsync();

                        var sqlInsert1 = "INSERT INTO `timkotodb`.`tempTeamPlayerId` (`teamId`, `playerId`) VALUES ";
                        var sqlValues1 = string.Join(",", teamPlayerIds.Select(_ => $"('{_.TeamId}', '{_.PlayerId}')"));

                        await dbSession.CreateSQLQuery($"{sqlInsert1} {sqlValues1};").ExecuteUpdateAsync();

                        var findSql = @$"select teamId, playerId
                                    from `timkotodb`.`tempTeamPlayerId` t1
                                        where not exists(
                                            select 1
                                        from `timkotodb`.`gamePlayer` t2
                                            where t1.playerId = t2.playerId and contestId = {contest.Id}
                                    );";

                        var missingTeamPlayerIds = (await dbSession.CreateSQLQuery(findSql)
                            .SetResultTransformer(Transformers.AliasToBean<TeamPlayerId>()).ListAsync<TeamPlayerId>()).ToList();

                        await dbSession.CreateSQLQuery("DROP TABLE `timkotodb`.`tempTeamPlayerId`;").ExecuteUpdateAsync();

                        await tx.CommitAsync();

                        dbSession.Close();
                        dbSession.Dispose();

                        if (missingTeamPlayerIds.Any())
                        {
                            await FixMissingPlayers(missingTeamPlayerIds, contest.Id, messages);
                        }

                        return $"Unmatched PlayerIds - { JsonConvert.SerializeObject(missingTeamPlayerIds)}";
                    }
                }

                return "Failed";
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                var result = GenericResponse.CreateErrorResponse(ex);
                result.Data = messages;
                return ex.Message;
            }
            finally
            {
                //TODO: logging
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

    }
}
