using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Timkoto.UsersApi.Enumerations
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LogType
    {
        [EnumMember(Value = "Information")]
        Information,

        [EnumMember(Value = "Error")]
        Error
    }
}
