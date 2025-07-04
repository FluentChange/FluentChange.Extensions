using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public static class RequestHelper
    {
        public static async Task<TOutputModel> GetRequestData<TOutputModel>(HttpRequest req, JsonSerializerSettings jsonSettings = null)
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var entityWrapped = JsonConvert.DeserializeObject<TOutputModel>(requestBody, jsonSettings);
            return entityWrapped;
        }
    }
}
