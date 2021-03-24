using NHibernate.Mapping.Attributes;
using System;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(WsConnection), Table = "wsConnection")]
    public class WsConnection
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "ConnectionId", Column = "connectionId", TypeType = typeof(string), Length = 100,
            Index = "ConnectionId_ix", NotNull = true)]
        public virtual string ConnectionId { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(string), Length = 10,
            Index = "operatorId_ix", NotNull = true)]
        public virtual string OperatorId { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }
    }
}
