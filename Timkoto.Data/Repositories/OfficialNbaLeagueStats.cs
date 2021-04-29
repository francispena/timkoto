using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(OfficialNbaLeagueStats), Table = "officialNbaLeagueStats")]
    public class OfficialNbaLeagueStats
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        public virtual long Id { get; set; }

        [Property(Name = "PersonId", Column = "personId", TypeType = typeof(int), Index = "personId_ix", NotNull = true)]
        public virtual int PersonId { get; set; }

        [Property(Name = "PlayerName", Column = "playerName", TypeType = typeof(string), Length = 60, Index = "playerName_ix", NotNull = true)]
        public virtual string PlayerName { get; set; }

        [Property(Name = "TeamId", Column = "teamId", TypeType = typeof(int), Index = "teamId_ix", NotNull = true)]
        public virtual int TeamId { get; set; }

        [Property(Name = "TeamAbbreviation", Column = "TeamAbbreviation", TypeType = typeof(string), Length = 60, Index = "teamAbbreviation_ix", NotNull = true)]
        public virtual string TeamAbbreviation { get; set; }

        [Property(Name = "FantasyPoints", Column = "fantasyPoints", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "fantasyPoints_ix")]
        public virtual decimal FantasyPoints { get; set; }

        [Property(Name = "FantasyRank", Column = "fantasyRank", TypeType = typeof(long), 
            Index = "fantasyRank_ix")]
        public virtual long FantasyRank { get; set; }
    }
}
