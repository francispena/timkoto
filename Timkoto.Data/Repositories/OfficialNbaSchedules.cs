using System;
using NHibernate.Mapping.Attributes;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(OfficialNbaSchedules), Table = "officialNbaSchedules")]
    public class OfficialNbaSchedules
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "GameDate", Column = "gameDate", TypeType = typeof(string), Index = "gameDate_ix", Length = 25, NotNull = false)]
        public virtual string GameDate { get; set; }

        [Property(Name = "GameId", Column = "gameId", TypeType = typeof(string), Index = "gameId_ix", Length = 60, NotNull = false)]
        public virtual string GameId { get; set; }

        [Property(Name = "GameDateEst", Column = "gameDateEst", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameDateEst { get; set; }

        [Property(Name = "GameTimeEst", Column = "gameTimeEst", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameTimeEst { get; set; }

        [Property(Name = "GameDateTimeEst", Column = "gameDateTimeEst", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameDateTimeEst { get; set; }

        [Property(Name = "GameDateUTC", Column = "gameDateUTC", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameDateUTC { get; set; }

        [Property(Name = "GameTimeUTC", Column = "gameTimeUTC", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameTimeUTC { get; set; }

        [Property(Name = "GameDateTimeUTC", Column = "gameDateTimeUTC", TypeType = typeof(DateTime), NotNull = false)]
        public virtual DateTime GameDateTimeUTC { get; set; }

        [Property(Name = "HomeTeamId", Column = "homeTeamId", TypeType = typeof(int), Index = "homeTeamId_ix", NotNull = true)]
        public virtual int HomeTeamId { get; set; }

        [Property(Name = "HomeTeamName", Column = "homeTeamName", TypeType = typeof(string), Length = 60)]
        public virtual string HomeTeamName { get; set; }

        [Property(Name = "VisitorTeamId", Column = "visitorTeamId", TypeType = typeof(int), Index = "visitorTeamId_ix", NotNull = true)]
        public virtual int VisitorTeamId { get; set; }

        [Property(Name = "VisitorTeamName", Column = "visitorTeamName", TypeType = typeof(string), Length = 60)]
        public virtual string VisitorTeamName { get; set; }

        [Property(Name = "Finished", Column = "finished", TypeType = typeof(bool), Index = "finished_ix")]
        public virtual bool Finished { get; set; }
    }
}
