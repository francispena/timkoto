﻿using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(PrizePool), Table = "prizePool")]
    public class PrizePool
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "ContestPrizeId", Column = "contestPrizeId", TypeType = typeof(long),
            Index = "contestPrizeId_ix", NotNull = true)]
        public virtual long ContestPrizeId { get; set; }

        [Property(Name = "FromRank", Column = "fromRank", TypeType = typeof(int),
            Index = "fromRank_ix", NotNull = true)]
        public virtual int FromRank { get; set; }

        [Property(Name = "ToRank", Column = "toRank", TypeType = typeof(int),
            Index = "toRank_ix", NotNull = true)]
        public virtual int ToRank { get; set; }
        
        [Property(Name = "Prize", Column = "prize", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "prize_ix", NotNull = true)]
        public virtual decimal Prize { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
