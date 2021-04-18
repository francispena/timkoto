using System;
using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class NbaApiSchedules
    {
        public Meta meta { get; set; }
        public LeagueSchedule leagueSchedule { get; set; }
    }
    
    public class Meta
    {
        public int version { get; set; }
        public string request { get; set; }
        public DateTime time { get; set; }
    }

    public class NationalTvBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class NationalRadioBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class HomeTvBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class HomeRadioBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class AwayTvBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class AwayRadioBroadcaster
    {
        public string broadcasterScope { get; set; }
        public string broadcasterMedia { get; set; }
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public string tapeDelayComments { get; set; }
        public int regionId { get; set; }
    }

    public class Broadcasters
    {
        public List<NationalTvBroadcaster> nationalTvBroadcasters { get; set; }
        public List<NationalRadioBroadcaster> nationalRadioBroadcasters { get; set; }
        public List<HomeTvBroadcaster> homeTvBroadcasters { get; set; }
        public List<HomeRadioBroadcaster> homeRadioBroadcasters { get; set; }
        public List<AwayTvBroadcaster> awayTvBroadcasters { get; set; }
        public List<AwayRadioBroadcaster> awayRadioBroadcasters { get; set; }
        public List<object> intlRadioBroadcasters { get; set; }
        public List<object> intlTvBroadcasters { get; set; }
    }

    public class HomeTeam
    {
        public int teamId { get; set; }
        public string teamName { get; set; }
        public string teamCity { get; set; }
        public string teamTricode { get; set; }
        public string teamSlug { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int score { get; set; }
        public int seed { get; set; }
    }

    public class AwayTeam
    {
        public int teamId { get; set; }
        public string teamName { get; set; }
        public string teamCity { get; set; }
        public string teamTricode { get; set; }
        public string teamSlug { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int score { get; set; }
        public int seed { get; set; }
    }

    public class PointsLeader
    {
        public int personId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int teamId { get; set; }
        public string teamCity { get; set; }
        public string teamName { get; set; }
        public string teamTricode { get; set; }
        public double points { get; set; }
    }

    public class OfficalNbaGame
    {
        public string gameId { get; set; }
        public string gameCode { get; set; }
        public int gameStatus { get; set; }
        public string gameStatusText { get; set; }
        public int gameSequence { get; set; }
        public DateTime gameDateEst { get; set; }
        public DateTime gameTimeEst { get; set; }
        public DateTime gameDateTimeEst { get; set; }
        public DateTime gameDateUTC { get; set; }
        public DateTime gameTimeUTC { get; set; }
        public DateTime gameDateTimeUTC { get; set; }
        public DateTime awayTeamTime { get; set; }
        public DateTime homeTeamTime { get; set; }
        public string day { get; set; }
        public int monthNum { get; set; }
        public int weekNumber { get; set; }
        public string weekName { get; set; }
        public bool ifNecessary { get; set; }
        public string seriesGameNumber { get; set; }
        public string seriesText { get; set; }
        public string arenaName { get; set; }
        public string arenaState { get; set; }
        public string arenaCity { get; set; }
        public string postponedStatus { get; set; }
        public Broadcasters broadcasters { get; set; }
        public HomeTeam homeTeam { get; set; }
        public AwayTeam awayTeam { get; set; }
        public List<PointsLeader> pointsLeaders { get; set; }
    }

    public class GameDate
    {
        public string gameDate { get; set; }
        public List<OfficalNbaGame> games { get; set; }
    }

    public class Week
    {
        public int weekNumber { get; set; }
        public string weekName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

    public class BroadcasterList
    {
        public int broadcasterId { get; set; }
        public string broadcasterDisplay { get; set; }
        public string broadcasterAbbreviation { get; set; }
        public int regionId { get; set; }
    }

    public class LeagueSchedule
    {
        public string seasonYear { get; set; }
        public string leagueId { get; set; }
        public List<GameDate> gameDates { get; set; }
        public List<Week> weeks { get; set; }
        public List<BroadcasterList> broadcasterList { get; set; }
    }
}
