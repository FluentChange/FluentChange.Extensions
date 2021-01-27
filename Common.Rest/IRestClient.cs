using System.Collections.Generic;

namespace FluentChange.Extensions.Common.Rest
{
    public interface IRestClient
    {
        T Delete<T>(string route, Dictionary<string, string> parameters = null);
        T Get<T>(string route, Dictionary<string, string> parameters = null);
        T Post<T>(string route, object requestBody, Dictionary<string, string> parameters = null);
        T Put<T>(string route, object requestBody, Dictionary<string, string> parameters = null);
    }
}
