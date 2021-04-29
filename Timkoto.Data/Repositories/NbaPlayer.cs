using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(NbaPlayer), Table = "nbaPlayer")]
    public class NbaPlayer
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(string), Length = 40)]
        public virtual string Id { get; set; }

        [Property(Name = "TeamId", Column = "teamId", TypeType = typeof(string), Length = 40,
            Index = "teamId_ix", NotNull = true)]
        public virtual string TeamId { get; set; }

        [Property(Name = "FirstName", Column = "firstName", TypeType = typeof(string), Length = 50,
            Index = "firstName_ix", NotNull = true)]
        public virtual string FirstName { get; set; }

        [Property(Name = "LastName", Column = "lastName", TypeType = typeof(string), Length = 50,
            Index = "lastName_ix", NotNull = true)]
        public virtual string LastName { get; set; }

        [Property(Name = "Jersey", Column = "jersey", TypeType = typeof(string), Length = 3,
            Index = "jersey_ix", NotNull = true)]
        public virtual string Jersey { get; set; }

        [Property(Name = "Position", Column = "position", TypeType = typeof(string), Length = 5,
            Index = "position_ix", NotNull = true)]
        public virtual string Position { get; set; }

        [Property(Name = "Salary", Column = "salary", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "salary_ix", NotNull = true)]
        public virtual decimal Salary { get; set; }

        [Property(Name = "Fppg", Column = "fppg", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "fppg_ix")]
        public virtual decimal Fppg { get; set; }

        [Property(Name = "Season", Column = "season", TypeType = typeof(string), Length = 5,
            Index = "season_ix", NotNull = true)]
        public virtual string Season { get; set; }
    }
}
