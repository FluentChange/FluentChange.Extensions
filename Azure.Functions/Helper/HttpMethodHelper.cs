using System;
using System.Net.Http;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public static class HttpMethodHelper
    {
        public static HttpMethod Get(string methodString)
        {
            switch (methodString.ToLower())
            {
                case "get": return HttpMethod.Get;
                case "post": return HttpMethod.Post;
                case "put": return HttpMethod.Put;
                case "delete": return HttpMethod.Delete;
                case "head": return HttpMethod.Head;
                case "options": return HttpMethod.Options;
                case "trace": return HttpMethod.Trace;
                case "patch": return HttpMethod.Patch;
                case "connect": return HttpMethod.Connect;
                default: throw new ArgumentException($"Unsupported HTTP method: {methodString}");
            }
        }
    }
}
