using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IRapidNbaStatistics
    {
        Task<bool> GetLiveStats(List<string> messages);

        Task<bool> GetFinalStats(List<string> messages);
    }
}