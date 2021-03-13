using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(NbaTeam), Table = "nbaTeam")]
    public class NbaTeam
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(string), Length = 40)]
        public virtual string Id { get; set; }

        [Property(Name = "FullName", Column = "fullName", TypeType = typeof(string), Length = 70,
            Index = "fullName_ix", NotNull = true)]
        public virtual string FullName { get; set; }

        [Property(Name = "NickName", Column = "nickName", TypeType = typeof(string), Length = 50,
            Index = "nickName_ix", NotNull = true)]
        public virtual string NickName { get; set; }

        [Property(Name = "Logo", Column = "logo", TypeType = typeof(string), Length = 65535, NotNull = true)]
        public virtual string Logo { get; set; }

        [Property(Name = "City", Column = "city", TypeType = typeof(string), Length = 50,
            Index = "city_ix", NotNull = true)]
        public virtual string City { get; set; }
    }
}
