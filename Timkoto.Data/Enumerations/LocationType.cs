using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Timkoto.Data.Enumerations
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LocationType
    {
        /// <summary>
        /// The operator
        /// </summary>
        [EnumMember(Value = "Home")] 
        Home = 1,

        /// <summary>
        /// The agent
        /// </summary>
        [EnumMember(Value = "Visitor")]
        Visitor = 2,
    }
}
