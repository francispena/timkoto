using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(NbaPlayer), Table = "nbaPlayer")]
    public class NbaPlayer
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(string), Length = 40)]
        public virtual string Id { get; set; }

        [Property(Name = "TeamId", Column = "TeamId", TypeType = typeof(string), Length = 40,
            Index = "teamId_ix", NotNull = true)]
        public virtual string TeamId { get; set; }

        [Property(Name = "FirstName", Column = "firstName", TypeType = typeof(string), Length = 50,
            Index = "firstName_ix", NotNull = true)]
        public virtual string FirstName { get; set; }

        [Property(Name = "LastName", Column = "nickName", TypeType = typeof(string), Length = 50,
            Index = "nickName_ix", NotNull = true)]
        public virtual string LastName { get; set; }

        [Property(Name = "Jersey", Column = "jersey", TypeType = typeof(string), Length = 3,
            Index = "jersey_ix", NotNull = true)]
        public virtual string Jersey { get; set; }

        [Property(Name = "Position", Column = "city", TypeType = typeof(string), Length = 5,
            Index = "city_ix", NotNull = true)]
        public virtual string Position { get; set; }

        [Property(Name = "Season", Column = "season", TypeType = typeof(string), Length = 5,
            Index = "season_ix", NotNull = true)]
        public virtual string Season { get; set; }
    }
}
