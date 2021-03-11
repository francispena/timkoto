using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Timkoto.Data.Enumerations
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserType
    {
        /// <summary>
        /// The operator
        /// </summary>
        [EnumMember(Value = "Operator")] 
        Operator = 1,

        /// <summary>
        /// The agent
        /// </summary>
        [EnumMember(Value = "Agent")]
        Agent = 2,

        /// <summary>
        /// The player
        /// </summary>
        [EnumMember(Value = "Player")]
        Player = 3
    }
}
