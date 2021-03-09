using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Timkoto.Data.Enumerations
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserType
    {
        /// <summary>
        /// The operator
        /// </summary>
        [EnumMember(Value = "Operator")] 
        Operator = 1,

        /// <summary>
        /// The master agent
        /// </summary>
        [EnumMember(Value = "MasterAgent")]
        MasterAgent = 2,

        /// <summary>
        /// The agent
        /// </summary>
        [EnumMember(Value = "Agent")]
        Agent = 3,

        /// <summary>
        /// The player
        /// </summary>
        [EnumMember(Value = "Player")]
        Player = 4
    }
}
