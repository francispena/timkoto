using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(Game), Table = "game")]
    public class Game
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(string), Length = 40)]
        public virtual string Id { get; set; }

        [Property(Name = "ContestId", Column = "contestId", TypeType = typeof(long),
            Index = "contestId_ix", NotNull = true)]
        public virtual long ContestId { get; set; }
      
        [Property(Name = "HTeamId", Column = "hTeamId", TypeType = typeof(string), Length = 40,
            Index = "hTeamId_ix", NotNull = true)]
        public virtual string HTeamId { get; set; }

        [Property(Name = "VTeamId", Column = "vTeamId", TypeType = typeof(string), Length = 40,
            Index = "vTeamId_ix", NotNull = true)]
        public virtual string VTeamId { get; set; }

        [Property(Name = "StartTime", Column = "startTime", TypeType = typeof(DateTime),
             Index = "startTime_ix")]
        public virtual DateTime StartTime { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
