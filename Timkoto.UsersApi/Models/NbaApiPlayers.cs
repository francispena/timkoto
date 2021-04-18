using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class NbaApiPlayers
    {
        public string resource { get; set; }
        public Parameters parameters { get; set; }
        public List<ResultSet> resultSets { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Parameters
    {
        public string LeagueID { get; set; }
        public string Season { get; set; }
        public int Historical { get; set; }
        public int TeamID { get; set; }
        public object Country { get; set; }
        public object College { get; set; }
        public object DraftYear { get; set; }
        public object DraftPick { get; set; }
        public object PlayerPosition { get; set; }
        public object Height { get; set; }
        public object Weight { get; set; }
        public object Active { get; set; }
        public object AllStar { get; set; }
    }

    public class ResultSet
    {
        public string name { get; set; }
        public List<string> headers { get; set; }
        public List<List<object>> rowSet { get; set; }
    }

    public static class PLAYERFIELDS
    {
        public const int PERSON_ID = 0;
        public const int PLAYER_LAST_NAME = 1;
        public const int PLAYER_FIRST_NAME = 2;
        public const int PLAYER_SLUG = 3;
        public const int TEAM_ID = 4;
        public const int TEAM_SLUG = 5;
        public const int IS_DEFUNCT = 6;
        public const int TEAM_CITY = 7;
        public const int TEAM_NAME = 8;
        public const int TEAM_ABBREVIATION = 9;
        public const int JERSEY_NUMBER = 10;
        public const int POSITION = 11;
        public const int HEIGHT = 12;
        public const int WEIGHT = 13;
        public const int COLLEGE = 14;
        public const int COUNTRY = 15;
        public const int DRAFT_YEAR = 16;
        public const int DRAFT_ROUND = 17;
        public const int DRAFT_NUMBER = 18;
        public const int ROSTER_STATUS = 19;
        public const int FROM_YEAR = 20;
        public const int TO_YEAR = 21;
        public const int PTS = 22;
        public const int REB = 23;
        public const int AST = 24;
        public const int STATS_TIMEFRAME = 25;
    }
}
