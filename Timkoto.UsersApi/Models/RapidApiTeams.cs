using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Models
{
    public class RapidApiTeams
    {
        public Api Api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Standard
    {
        public string confName { get; set; }
        public string divName { get; set; }
    }

    public class Leagues
    {
        public Standard standard { get; set; }
    }

    public class Team
    {
        public string city { get; set; }
        public string fullName { get; set; }
        public string teamId { get; set; }
        public string nickname { get; set; }
        public string logo { get; set; }
        public string shortName { get; set; }
        public string allStar { get; set; }
        public string nbaFranchise { get; set; }
        public Leagues leagues { get; set; }
    }

    public class Api
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<Team> teams { get; set; }
    }

    public class Root
    {
        public Api api { get; set; }
    }
}
