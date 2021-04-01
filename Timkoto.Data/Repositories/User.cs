using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using System;
using System.Text.Json.Serialization;
using Timkoto.Data.Enumerations;

namespace Timkoto.Data.Repositories
{
    [Class(NameType = typeof(User), Table = "user")]
    public class User
    {
        [Id(0, Name = "Id", Column = "id", TypeType = typeof(long))]
        [Generator(1, Class = "identity")]
        public virtual long Id { get; set; }

        [Property(Name = "Email", Column = "email", TypeType = typeof(string), Length = 30,
            Index = "email_ix", Unique = true, NotNull = true)]
        public virtual string Email { get; set; }

        [Property(Name = "UserName", Column = "userName", TypeType = typeof(string), Length = 30,
            Index = "userName_ix", NotNull = true)]
        public virtual string UserName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(Name = "UserType", Column = "userType", TypeType = typeof(EnumStringType<UserType>),
            Index = "userType_ix", Length = 20, NotNull = true)]
        public virtual UserType UserType { get; set; }

        [Property(Name = "IsActive", Column = "isActive", TypeType = typeof(bool), 
            Index = "isActive_ix", NotNull = true)]
        public virtual bool IsActive { get; set; }

        [Property(Name = "PhoneNumber", Column = "phoneNumber", TypeType = typeof(string), Length = 10,
            Index = "phoneNumber_ix", NotNull = false)]
        public virtual string PhoneNumber { get; set; }

        [Property(Name = "AgentId", Column = "agentId", TypeType = typeof(long), 
            Index = "agentId_ix", NotNull = false)]
        public virtual long AgentId { get; set; }

        [Property(Name = "OperatorId", Column = "operatorId", TypeType = typeof(long), 
            Index = "operatorId_ix", NotNull = false)]
        public virtual long OperatorId { get; set; }

        [Property(Name = "Points", Column = "points", TypeType = typeof(decimal), Precision = 10, Scale = 2,
            Index = "points_ix")]
        public virtual decimal Points { get; set; }

        [Property(Name = "PasswordResetCode", Column = "passwordResetCode", TypeType = typeof(string), Length = 10,
            Index = "passwordResetCode_ix")]
        public virtual string PasswordResetCode { get; set; }

        [Property(Name = "AccessToken", Column = "accessToken", TypeType = typeof(string), 
            Index = "accessToken_ix")]
        public virtual string AccessToken { get; set; }

        [Property(Name = "CreateDateTime", Column = "createDateTime", TypeType = typeof(DateTime), 
            Generated = PropertyGeneration.Insert, Index = "createDateTime_ix")]
        public virtual DateTime CreateDateTime { get; set; }

        [Property(Name = "UpdateDateTime", Column = "updateDateTime", TypeType = typeof(DateTime),
             Index = "updateDateTime_ix", NotNull = false)]
        public virtual DateTime UpdateDateTime { get; set; }
    }
}
