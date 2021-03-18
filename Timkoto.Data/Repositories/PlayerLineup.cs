using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(PlayerLineup), Table = "playerLineup")]
    public class PlayerLineup
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(long),
            Index = "operatorId_ix", NotNull = true)]
        public virtual long OperatorId { get; set; }

        [Property(Name = "ContestId", Column = "contestId", TypeType = typeof(long),
            Index = "contestId_ix", NotNull = true)]
        public virtual long ContestId { get; set; }

        [Property(Name = "UserId", Column = "userId", TypeType = typeof(long),
            Index = "userId_ix", NotNull = true)]
        public virtual long UserId { get; set; }

        [Property(Name = "PlayerTeamId", Column = "playerTeamId", TypeType = typeof(long),
            Index = "playerTeamId_ix", NotNull = true)]
        public virtual long PlayerTeamId { get; set; }

        [Property(Name = "PlayerId", Column = "playerId", TypeType = typeof(string), Length = 40,
            Index = "playerId_ix", NotNull = true)]
        public virtual string PlayerId { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
