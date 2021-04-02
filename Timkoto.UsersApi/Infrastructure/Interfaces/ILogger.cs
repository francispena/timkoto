using System.Collections.Generic;
using Timkoto.UsersApi.Enumerations;

namespace Timkoto.UsersApi.Infrastructure.Interfaces
{
    public interface ILogger
    {
        void Log(string header, List<string> messages, LogType logType);
    }
}