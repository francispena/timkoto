using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IRapidNbaStatistics
    {
        Task<bool> GetScores(List<string> messages);
    }
}