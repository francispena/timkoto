using System.Collections.Generic;

namespace Timkoto.UsersApi.Models.Player
{
    public class RapidApiPlayers
    {
        public Api Api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Standard
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class Sacramento
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class Vegas
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class Leagues
    {
        public Standard standard { get; set; }
        public Sacramento sacramento { get; set; }
        public Vegas vegas { get; set; }
    }

    public class Player
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string teamId { get; set; }
        public string yearsPro { get; set; }
        public string collegeName { get; set; }
        public string country { get; set; }
        public string playerId { get; set; }
        public string dateOfBirth { get; set; }
        public string affiliation { get; set; }
        public string startNba { get; set; }
        public string heightInMeters { get; set; }
        public string weightInKilograms { get; set; }
        public Leagues leagues { get; set; }
    }

    public class Api
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<Player> players { get; set; }
    }

}
