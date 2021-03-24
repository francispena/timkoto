using NHibernate.Mapping.Attributes;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Type;
using Timkoto.Data.Enumerations;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(Contest), Table = "contest")]
    public class Contest
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "GameDate", Column = "gameDate", TypeType = typeof(string), Length = 10,
            Index = "gameDate_ix", NotNull = true)]
        public virtual string GameDate { get; set; }

        [Property(Name = "Sport", Column = "sport", TypeType = typeof(string), Length = 30,
            Index = "sport_ix", NotNull = true)]
        public virtual string Sport { get; set; }

        [Property(Name = "SalaryCap", Column = "salaryCap", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "salaryCap_ix", NotNull = true)]
        public virtual decimal SalaryCap { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "ContestState", Column = "contestState", TypeType = typeof(EnumStringType<ContestState>),
            Index = "contestState_ix", Length = 20, NotNull = true)]
        public virtual ContestState ContestState { get; set; }

        [Property(Name = "EntryPoints", Column = "entryPoints", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "entryPoints_ix", NotNull = true)]
        public virtual decimal EntryPoints { get; set; }
        
        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
