using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IRapidNbaStatistics
    {
        Task<string> GetLiveStats(List<string> messages);

        Task<string> GetLiveStats2(List<string> messages);

        Task<string> UpdateGameIds(List<string> messages);
    }
}