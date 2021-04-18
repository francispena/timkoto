using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(OfficialNbaPlayerStats), Table = "officialNbaPlayerStats")]
    public class OfficialNbaPlayerStats
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        public virtual long Id { get; set; }

        [Property(Name = "PersonId", Column = "personId", TypeType = typeof(int), Index = "personId_ix", NotNull = true)]
        public virtual int PersonId { get; set; }

        [Property(Name = "TeamId", Column = "teamId", TypeType = typeof(int), Index = "teamId_ix", NotNull = true)]
        public virtual int TeamId { get; set; }

        [Property(Name = "Location", Column = "location", TypeType = typeof(string), Length = 60, NotNull = true)]
        public virtual string Location { get; set; }

        [Property(Name = "TeamName", Column = "teamName", TypeType = typeof(string), Length = 60, Index = "teamName_ix", NotNull = true)]
        public virtual string TeamName { get; set; }

        [Property(Name = "FirstName", Column = "firstName", TypeType = typeof(string), Length = 60, Index = "firstName_ix", NotNull = true)]
        public virtual string FirstName { get; set; }

        [Property(Name = "FamilyName", Column = "familyName", TypeType = typeof(string), Length = 60, Index = "familyName_ix", NotNull = true)]
        public virtual string FamilyName { get; set; }

        [Property(Name = "Points", Column = "points", TypeType = typeof(int))]
        public virtual int Points { get; set; }

        [Property(Name = "Assists", Column = "assists", TypeType = typeof(int))]
        public virtual int Assists { get; set; }

        [Property(Name = "Blocks", Column = "blocks", TypeType = typeof(int))]
        public virtual int Blocks { get; set; }

        [Property(Name = "Steals", Column = "steals", TypeType = typeof(int))]
        public virtual int Steals { get; set; }

        [Property(Name = "ReboundsTotal", Column = "reboundsTotal", TypeType = typeof(int))]
        public virtual int ReboundsTotal { get; set; }

        [Property(Name = "Turnovers", Column = "turnovers", TypeType = typeof(int))]
        public virtual int Turnovers { get; set; }
    }
}
