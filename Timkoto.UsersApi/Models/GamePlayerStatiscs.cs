using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class GamePlayerStatiscs
    {
        public Api api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Statistic
    {
        public string gameId { get; set; }
        public string teamId { get; set; }
        public string points { get; set; }
        public string pos { get; set; }
        public string min { get; set; }
        public string fgm { get; set; }
        public string fga { get; set; }
        public string fgp { get; set; }
        public string ftm { get; set; }
        public string fta { get; set; }
        public string ftp { get; set; }
        public string tpm { get; set; }
        public string tpa { get; set; }
        public string tpp { get; set; }
        public string offReb { get; set; }
        public string defReb { get; set; }
        public string totReb { get; set; }
        public string assists { get; set; }
        public string pFouls { get; set; }
        public string steals { get; set; }
        public string turnovers { get; set; }
        public string blocks { get; set; }
        public string plusMinus { get; set; }
        public string playerId { get; set; }
    }

    public class Api
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<Statistic> statistics { get; set; }
    }
}
