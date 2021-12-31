using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public class InternalRestClient : AbstractInternalRestClient
    {
        private string bearerToken;
        private readonly HttpClient http;

        public InternalRestClient(string endpoint) : base()
        {
            if (!endpoint.StartsWith("https://") && !endpoint.StartsWith("http://")) endpoint = "https://" + endpoint;
            if (!endpoint.EndsWith("/")) endpoint = endpoint + "/";
            if (!endpoint.EndsWith("api/")) endpoint = endpoint + "api/";
            http = new HttpClient() { BaseAddress = new Uri(endpoint) };
        }

        protected override Task<HttpResponseMessage> GetImplementation(string route, Dictionary<string, string> parameters)
        {
            route = ReplaceParams(route, parameters);
            return http.GetAsync(route);
        }
        protected override Task<HttpResponseMessage> PostImplementation(string route, HttpContent content, Dictionary<string, string> parameters)
        {
            route = ReplaceParams(route, parameters);
            return http.PostAsync(route, content);
        }
        protected override Task<HttpResponseMessage> PutImplementation(string route, HttpContent content, Dictionary<string, string> parameters)
        {
            route = ReplaceParams(route, parameters);
            return http.PutAsync(route, content);
        }
        protected override Task<HttpResponseMessage> DeleteImplementation(string route, Dictionary<string, string> parameters)
        {
            route = ReplaceParams(route, parameters);
            return http.DeleteAsync(route);
        }

        protected override void SetHeader(string key, string value)
        {
            if (!http.DefaultRequestHeaders.Contains(key))
            {
                http.DefaultRequestHeaders.Add(key, value);
            }
            else
            {
                http.DefaultRequestHeaders.Remove(key);
                http.DefaultRequestHeaders.Add(key, value);
            }
        }

        protected override void RemoveHeader(string key)
        {
            http.DefaultRequestHeaders.Remove(key);
        }

        protected override bool ExistHeader(string key)
        {
            return http.DefaultRequestHeaders.Contains(key);
        }

    
    }
}
