using NHibernate.Mapping.Attributes;
using System;

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
        
        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
