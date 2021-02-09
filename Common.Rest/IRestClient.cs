using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public interface IRestClient
    {
        Task<T> Delete<T>(string route, Dictionary<string, string> parameters = null);
        Task<T> Get<T>(string route, Dictionary<string, string> parameters = null);
        Task<T> Post<T>(string route, object requestBody, Dictionary<string, string> parameters = null);
        Task<T> Put<T>(string route, object requestBody, Dictionary<string, string> parameters = null);
    }
}
