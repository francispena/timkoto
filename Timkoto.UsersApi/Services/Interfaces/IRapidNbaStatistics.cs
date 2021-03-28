using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IRapidNbaStatistics
    {
        Task<string> GetLiveStats(List<string> messages);

        Task<string> GetFinalStats(List<string> messages);
    }
}