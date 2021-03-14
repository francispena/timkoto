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

        [Property(Name = "GameId", Column = "gameId", TypeType = typeof(long),
            Index = "gameId_ix", NotNull = true)]
        public virtual long GameId { get; set; }

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

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
