using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Timkoto.Data.Enumerations
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContestState
    {
        /// <summary>
        /// The operator
        /// </summary>
        [EnumMember(Value = "Scheduled")]
        Scheduled = 1,

        /// <summary>
        /// The operator
        /// </summary>
        [EnumMember(Value = "Upcoming")] 
        Upcoming = 2,

        /// <summary>
        /// The agent
        /// </summary>
        [EnumMember(Value = "Ongoing")]
        Ongoing = 3,

        /// <summary>
        /// The player
        /// </summary>
        [EnumMember(Value = "Finished")]
        Finished = 4
    }
}
