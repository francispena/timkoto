﻿using System;
using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class RapidApiGames
    {
        public RapidApiGamesApi Api { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RapidApiGamesScore
    {
        public string points { get; set; }
    }

    public class RapidApiGamesVTeam
    {
        public string teamId { get; set; }
        public string shortName { get; set; }
        public string fullName { get; set; }
        public string nickName { get; set; }
        public string logo { get; set; }
        public RapidApiGamesScore score { get; set; }
    }

    public class RapidApiGamesHTeam
    {
        public string teamId { get; set; }
        public string shortName { get; set; }
        public string fullName { get; set; }
        public string nickName { get; set; }
        public string logo { get; set; }
        public RapidApiGamesScore score { get; set; }
    }

    public class RapidApiGamesGame
    {
        public string seasonYear { get; set; }
        public string league { get; set; }
        public string gameId { get; set; }
        public DateTime startTimeUTC { get; set; }
        public string endTimeUTC { get; set; }
        public string arena { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string clock { get; set; }
        public string gameDuration { get; set; }
        public string currentPeriod { get; set; }
        public string halftime { get; set; }
        public string EndOfPeriod { get; set; }
        public string seasonStage { get; set; }
        public string statusShortGame { get; set; }
        public string statusGame { get; set; }
        public RapidApiGamesVTeam vTeam { get; set; }
        public RapidApiGamesHTeam hTeam { get; set; }
    }

        public class RapidApiGamesApi
    {
        public int status { get; set; }
        public string message { get; set; }
        public int results { get; set; }
        public List<string> filters { get; set; }
        public List<RapidApiGamesGame> games { get; set; }
    }
    
}
