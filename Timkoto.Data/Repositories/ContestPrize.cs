using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(ContestPrize), Table = "contestPrize")]
    public class ContestPrize
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "TotalPackage", Column = "totalPackage", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "totalPackage_ix", NotNull = true)]
        public virtual decimal TotalPackage { get; set; }

        [Property(Name = "Tag", Column = "tag", TypeType = typeof(string),
            Index = "tag_ix", NotNull = true)]
        public virtual decimal Tag { get; set; }
        
        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
