using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Infrastructure.Interfaces
{
    public interface IHttpService
    {
        Task<TResponse> GetAsync<TResponse>(string requestUri, Dictionary<string, string> headerKeyValues);
    }
}