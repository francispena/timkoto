using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class RapidApiPlayers
    {
        public RapidApiPlayersApi Api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RapidApiPlayersStandard
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class RapidApiPlayersSacramento
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class RapidApiPlayersVegas
    {
        public string jersey { get; set; }
        public string active { get; set; }
        public string pos { get; set; }
    }

    public class RapidApiPlayersLeagues
    {
        public RapidApiPlayersStandard standard { get; set; }
        public RapidApiPlayersSacramento sacramento { get; set; }
        public RapidApiPlayersVegas vegas { get; set; }
    }

    public class RapidApiPlayersPlayer
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
        public RapidApiPlayersLeagues leagues { get; set; }
    }

    public class RapidApiPlayersApi
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<RapidApiPlayersPlayer> players { get; set; }
    }

}
