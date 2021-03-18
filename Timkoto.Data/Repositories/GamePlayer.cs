using NHibernate.Mapping.Attributes;
using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using NHibernate.Type;
using Timkoto.Data.Enumerations;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(GamePlayer), Table = "gamePlayer")]
    public class GamePlayer
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "ContestId", Column = "contestId", TypeType = typeof(long),
            Index = "contestId_ix", NotNull = true)]
        public virtual long ContestId { get; set; }

        [Property(Name = "GameId", Column = "GameId", TypeType = typeof(string), Length = 40,
            Index = "gameId_ix", NotNull = true)]
        public virtual string GameId { get; set; }

        [Property(Name = "TeamId", Column = "teamId", TypeType = typeof(string), Length = 40,
            Index = "teamId_ix", NotNull = true)]
        public virtual string TeamId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "TeamLocation", Column = "teamLocation", TypeType = typeof(EnumStringType<LocationType>),
            Index = "teamLocation_ix", Length = 10, NotNull = true)]
        public virtual LocationType TeamLocation { get; set; }

        [Property(Name = "PlayerId", Column = "playerId", TypeType = typeof(string), Length = 40,
            Index = "playerId_ix", NotNull = true)]
        public virtual string PlayerId { get; set; }

        [Property(Name = "Salary", Column = "salary", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "salary_ix", NotNull = true)]
        public virtual decimal Salary { get; set; }

        [Property(Name = "Points", Column = "points", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "points_ix", NotNull = true)]
        public virtual decimal Points { get; set; }

        [Property(Name = "Rebounds", Column = "rebounds", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "rebounds_ix", NotNull = true)]
        public virtual decimal Rebounds { get; set; }

        [Property(Name = "Assists", Column = "assists", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "assists_ix", NotNull = true)]
        public virtual decimal Assists { get; set; }

        [Property(Name = "Steals", Column = "steals", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "steals_ix", NotNull = true)]
        public virtual decimal Steals { get; set; }

        [Property(Name = "Blocks", Column = "blocks", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "blocks_ix", NotNull = true)]
        public virtual decimal Blocks { get; set; }

        [Property(Name = "TurnOvers", Column = "turnOvers", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "turnOvers_ix", NotNull = true)]
        public virtual decimal TurnOvers { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
