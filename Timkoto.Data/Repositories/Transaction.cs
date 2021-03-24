using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using System;
using System.Text.Json.Serialization;
using Timkoto.Data.Enumerations;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(Transaction), Table = "transaction")]
    public class Transaction
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(long),
            Index = "operatorId_ix", NotNull = true)]
        public virtual long OperatorId { get; set; }

        [Property(Name = "AgentId", Column = "agentId", TypeType = typeof(long),
            Index = "agentId_ix", NotNull = true)]
        public virtual long AgentId { get; set; }

        [Property(Name = "UserId", Column = "userId", TypeType = typeof(long),
            Index = "userId_ix", NotNull = true)]
        public virtual long UserId { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "UserType", Column = "userType", TypeType = typeof(EnumStringType<UserType>),
            Index = "userType_ix", Length = 20, NotNull = true)]
        public virtual UserType UserType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "TransactionType", Column = "transactionType", TypeType = typeof(EnumStringType<TransactionType>),
            Index = "transactionType_ix", Length = 30, NotNull = true)]
        public virtual TransactionType TransactionType { get; set; }

        [Property(Name = "Amount", Column = "amount", TypeType = typeof(decimal), Precision = 10, Scale = 2 ,
            Index = "amount_ix", NotNull = true)]
        public virtual decimal Amount { get; set; }

        [Property(Name = "Balance", Column = "balance", TypeType = typeof(decimal), Precision = 10, Scale = 2,
            Index = "balance_ix", NotNull = true)]
        public virtual decimal Balance { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime),
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }

    }
}
