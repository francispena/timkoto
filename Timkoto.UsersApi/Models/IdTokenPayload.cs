using Newtonsoft.Json;

namespace Timkoto.UsersApi.Models
{
    public class IdTokenPayload
    {
        public string sub { get; set; }
        public string aud { get; set; }
        public bool email_verified { get; set; }
        public string event_id { get; set; }
        public string token_use { get; set; }
        public int auth_time { get; set; }
        public string iss { get; set; }

        [JsonProperty("cognito:username")]
        public string CognitoUsername { get; set; }
        public int exp { get; set; }
        public int iat { get; set; }
        public string email { get; set; }
    }
}
