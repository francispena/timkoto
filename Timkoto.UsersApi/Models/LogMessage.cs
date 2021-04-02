using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class LogMessage
    {
        public string Header { get; set; }
        
        public string Messages { get; set; }

        public LogType Type { get; set; }

    }
}
