using FluentChange.Extensions.Common.Rest;
using System;
using System.Collections.Generic;

namespace SampleFunctions.Tests
{
    public class InternalClient : IRestClient
    {

        private RestSharp.RestClient rest;
          public InternalClient(string endpoint)
        {
            if (!endpoint.StartsWith("https://") && !endpoint.StartsWith("http://")) endpoint = "https://" + endpoint;
            if (!endpoint.EndsWith("/")) endpoint = endpoint + "/";
            if (!endpoint.EndsWith("api")) endpoint = endpoint + "api";
            rest = new RestSharp.RestClient(endpoint);
            //routeParams = new Dictionary<string, string>();
        }

        public T Get<T>(string route, Dictionary<string, string> parameters = null)
        {
            route = ReplaceParams(route, parameters);
            var request = new RestSharp.RestRequest(route, RestSharp.Method.GET);

            try
            {
                var response2 = rest.Execute(request);
                var response = rest.Execute<T>(request);
                return response.Data;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          

            return default(T);

        }
        public T Post<T>(string route, object requestBody, Dictionary<string, string> parameters = null)
        { 
            route = ReplaceParams(route, parameters);

            var request = new RestSharp.RestRequest(route, RestSharp.Method.POST);
            request.AddJsonBody(requestBody);
            var response = rest.Execute<T>(request);
            return response.Data;
        }

        public T Put<T>(string route, object requestBody, Dictionary<string, string> parameters = null)
        {
            route = ReplaceParams(route, parameters);

            var request = new RestSharp.RestRequest(route, RestSharp.Method.POST);
            request.AddJsonBody(requestBody);

            var response = rest.Execute<T>(request);
            return response.Data;
        }

        public T Delete<T>(string route, Dictionary<string, string> parameters = null)
        {
            route = ReplaceParams(route, parameters);
            var request = new RestSharp.RestRequest(route, RestSharp.Method.DELETE);

            var response = rest.Execute<T>(request);
            return response.Data;
        }


        private string ReplaceParams(string route, Dictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    route = route.Replace("{" + param.Key + "}", param.Value);
                }
            }

            return route;
        }
    }

}
