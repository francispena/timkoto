using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(Wager), Table = "wager")]
    public class Wager
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "ContestId", Column = "contestId", TypeType = typeof(long),
            Index = "contestId_ix", NotNull = true)]
        public virtual long ContestId { get; set; }

        [Property(Name = "UserId", Column = "userId", TypeType = typeof(long),
            Index = "userId_ix", NotNull = true)]
        public virtual long UserId { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(long),
            Index = "operatorId_ix", NotNull = true)]
        public virtual long OperatorId { get; set; }

        [Property(Name = "AgentId", Column = "agentId", TypeType = typeof(long),
            Index = "agentId_ix", NotNull = true)]
        public virtual long AgentId { get; set; }
        
        [Property(Name = "Amount", Column = "amount", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "amount_ix", NotNull = true)]
        public virtual decimal Amount { get; set; }

        [Property(Name = "AgentCommission", Column = "agentCommission", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "agentCommission_ix", NotNull = true)]
        public virtual decimal AgentCommission { get; set; }

        [Property(Name = "Prize", Column = "prize", TypeType = typeof(decimal), Scale = 2, Precision = 10,
            Index = "prize_ix", NotNull = true)]
        public virtual bool Prize { get; set; }

        [Property(Name = "IsPaid", Column = "isPaid", TypeType = typeof(bool),
            Index = "isPaid_ix", NotNull = true)]
        public virtual bool IsPaid { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime),
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
