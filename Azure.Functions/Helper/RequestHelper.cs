using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public static class RequestHelper
    {
        public static async Task<TOutputModel?> GetRequestData<TOutputModel>(HttpRequest req, JsonSerializerOptions? jsonOptions = null)
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            return JsonHelper.Deserialize<TOutputModel>(requestBody, jsonOptions);
        }
    }
}
