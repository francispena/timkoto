using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using System;
using System.Text.Json.Serialization;
using Timkoto.Data.Enumerations;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(RegistrationCode), Table = "registrationCode")]
    public class RegistrationCode
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "Code", Column = "code", TypeType = typeof(string), Length = 50,
            Index = "code_ix", NotNull = true)]
        public virtual string Code { get; set; }

        [Property(Name = "UserName", Column = "userName", TypeType = typeof(string), Length = 30,
            Index = "userName_ix", NotNull = true)]
        public virtual string UserName { get; set; }

        [Property(Name = "AgentId", Column = "agentId", TypeType = typeof(long),
            Index = "agentId_ix", NotNull = false)]
        public virtual long AgentId { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(long),
            Index = "operatorId_ix", NotNull = false)]
        public virtual long OperatorId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "UserType", Column = "userType", TypeType = typeof(EnumStringType<UserType>),
            Index = "userType_ix", Length = 20, NotNull = true)]
        public virtual UserType UserType { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime),
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }

        [Property(Name = "IsActive", Column = "isActive", TypeType = typeof(bool),
            Index = "isActive_ix", NotNull = true)]
        public virtual bool IsActive { get; set; }
    }
}
