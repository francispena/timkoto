using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class NbaApiLeagueStats
    {
        public string resource { get; set; }
        public NbaApiLeagueStatsParameters parameters { get; set; }
        public List<NbaApiLeagueStatsResultSet> resultSets { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class NbaApiLeagueStatsParameters
    {
        public string MeasureType { get; set; }
        public string PerMode { get; set; }
        public string PlusMinus { get; set; }
        public string PaceAdjust { get; set; }
        public string Rank { get; set; }
        public string LeagueID { get; set; }
        public string Season { get; set; }
        public string SeasonType { get; set; }
        public int PORound { get; set; }
        public object Outcome { get; set; }
        public object Location { get; set; }
        public int Month { get; set; }
        public object SeasonSegment { get; set; }
        public object DateFrom { get; set; }
        public object DateTo { get; set; }
        public int OpponentTeamID { get; set; }
        public object VsConference { get; set; }
        public object VsDivision { get; set; }
        public int TeamID { get; set; }
        public object Conference { get; set; }
        public object Division { get; set; }
        public object GameSegment { get; set; }
        public int Period { get; set; }
        public object ShotClockRange { get; set; }
        public int LastNGames { get; set; }
        public object GameScope { get; set; }
        public object PlayerExperience { get; set; }
        public object PlayerPosition { get; set; }
        public object StarterBench { get; set; }
        public object DraftYear { get; set; }
        public object DraftPick { get; set; }
        public object College { get; set; }
        public object Country { get; set; }
        public object Height { get; set; }
        public object Weight { get; set; }
        public int TwoWay { get; set; }
    }

    public class NbaApiLeagueStatsResultSet
    {
        public string name { get; set; }
        public List<string> headers { get; set; }
        public List<List<object>> rowSet { get; set; }
    }
}
