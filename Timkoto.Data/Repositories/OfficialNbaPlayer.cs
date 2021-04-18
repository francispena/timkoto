using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(OfficialNbaPlayer), Table = "officialNbaPlayer")]
    public class OfficialNbaPlayer
    {
        [Id(0, Name = "PERSON_ID", Column = "PERSON_ID", TypeType = typeof(long))]
        public virtual long PERSON_ID { get; set; }

        [Property(Name = "PLAYER_LAST_NAME", Column = "PLAYER_LAST_NAME", TypeType = typeof(string), Length = 60, Index = "PLAYER_LAST_NAME_ix")]
        public virtual string PLAYER_LAST_NAME { get; set; }

        [Property(Name = "PLAYER_FIRST_NAME", Column = "PLAYER_FIRST_NAME", TypeType = typeof(string), Length = 60, Index = "PLAYER_FIRST_NAME_ix")]
        public virtual string PLAYER_FIRST_NAME { get; set; }

        [Property(Name = "PLAYER_SLUG", Column = "PLAYER_SLUG", TypeType = typeof(string), Length = 60)]
        public virtual string PLAYER_SLUG { get; set; }

        [Property(Name = "TEAM_ID", Column = "TEAM_ID", TypeType = typeof(long), Index = "TEAM_ID_ix", NotNull = true)]
        public virtual long TEAM_ID { get; set; }

        [Property(Name = "TEAM_SLUG", Column = "TEAM_SLUG", TypeType = typeof(string), Length = 60)]
        public virtual string TEAM_SLUG { get; set; }

        [Property(Name = "IS_DEFUNCT", Column = "IS_DEFUNCT", TypeType = typeof(long))]
        public virtual long IS_DEFUNCT { get; set; }

        [Property(Name = "TEAM_CITY", Column = "TEAM_CITY", TypeType = typeof(string), Length = 60)]
        public virtual string TEAM_CITY { get; set; }

        [Property(Name = "TEAM_NAME", Column = "TEAM_NAME", TypeType = typeof(string), Length = 60, Index = "TEAM_NAME_ix")]
        public virtual string TEAM_NAME { get; set; }

        [Property(Name = "TEAM_ABBREVIATION", Column = "TEAM_ABBREVIATION", TypeType = typeof(string), Length = 60, Index = "TEAM_ABBREVIATION_ix")]
        public virtual string TEAM_ABBREVIATION { get; set; }

        [Property(Name = "JERSEY_NUMBER", Column = "JERSEY_NUMBER", TypeType = typeof(string), Length = 60)]
        public virtual string JERSEY_NUMBER { get; set; }

        [Property(Name = "POSITION", Column = "POSITION", TypeType = typeof(string), Length = 60, Index = "POSITION_ix")]
        public virtual string POSITION { get; set; }

        [Property(Name = "HEIGHT", Column = "HEIGHT", TypeType = typeof(string), Length = 60)]
        public virtual string HEIGHT { get; set; }

        [Property(Name = "WEIGHT", Column = "WEIGHT", TypeType = typeof(string), Length = 60)]
        public virtual string WEIGHT { get; set; }

        [Property(Name = "COLLEGE", Column = "COLLEGE", TypeType = typeof(string), Length = 60)]
        public virtual string COLLEGE { get; set; }

        [Property(Name = "COUNTRY", Column = "COUNTRY", TypeType = typeof(string), Length = 60)]
        public virtual string COUNTRY { get; set; }

        [Property(Name = "DRAFT_YEAR", Column = "DRAFT_YEAR", TypeType = typeof(long))]
        public virtual long DRAFT_YEAR { get; set; }

        [Property(Name = "DRAFT_ROUND", Column = "DRAFT_ROUND", TypeType = typeof(long))]
        public virtual long DRAFT_ROUND { get; set; }

        [Property(Name = "DRAFT_NUMBER", Column = "DRAFT_NUMBER", TypeType = typeof(long))]
        public virtual long DRAFT_NUMBER { get; set; }

        [Property(Name = "ROSTER_STATUS", Column = "ROSTER_STATUS", TypeType = typeof(double))]
        public virtual double ROSTER_STATUS { get; set; }

        [Property(Name = "FROM_YEAR", Column = "FROM_YEAR", TypeType = typeof(string), Length = 60)]
        public virtual string FROM_YEAR { get; set; }

        [Property(Name = "TO_YEAR", Column = "TO_YEAR", TypeType = typeof(string), Length = 60)]
        public virtual string TO_YEAR { get; set; }

        [Property(Name = "PTS", Column = "PTS", TypeType = typeof(double))]
        public virtual double PTS { get; set; }

        [Property(Name = "REB", Column = "REB", TypeType = typeof(double))]
        public virtual double REB { get; set; }

        [Property(Name = "AST", Column = "AST", TypeType = typeof(double))]
        public virtual double AST { get; set; }

        [Property(Name = "STATS_TIMEFRAME", Column = "STATS_TIMEFRAME", TypeType = typeof(string), Length = 60)]
        public virtual string STATS_TIMEFRAME { get; set; }
        
    }
}
