using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class RapidApiTeams
    {
        public RapidApiTeamsApi Api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RapidApiTeamsStandard
    {
        public string confName { get; set; }
        public string divName { get; set; }
    }

    public class RapidApiTeamsLeagues
    {
        public RapidApiTeamsStandard standard { get; set; }
    }

    public class RapidApiTeamsTeam
    {
        public string city { get; set; }
        public string fullName { get; set; }
        public string teamId { get; set; }
        public string nickname { get; set; }
        public string logo { get; set; }
        public string shortName { get; set; }
        public string allStar { get; set; }
        public string nbaFranchise { get; set; }
        public RapidApiTeamsLeagues leagues { get; set; }
    }

    public class RapidApiTeamsApi
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<RapidApiTeamsTeam> teams { get; set; }
    }
}
