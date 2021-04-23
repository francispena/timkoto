using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IOfficialNbaStatistics
    {
        Task<string> GetLiveStats(List<string> messages);
    }
}