using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;

namespace FluentChange.Extensions.Azure.Functions.Helper
{

    public static class ResponseHelperIsolated
    {
        public static HttpResponseData CreateEmptyResponse(HttpRequestData request, HttpStatusCode statuscode)
        {
            var response = request.CreateResponse(statuscode);
            return response;
        }
        public static HttpResponseData CreateJsonResponse(HttpRequestData request, object responseData, HttpStatusCode statuscode, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var json = "";
            if (jsonSerializerSettings == null)
            {
                json = JsonConvert.SerializeObject(responseData);
            }
            else
            {
                json = JsonConvert.SerializeObject(responseData, jsonSerializerSettings);
            }
                        
            var response = request.CreateResponse(statuscode);
            
            if (response.Headers.Contains("Content-Type"))
            {
                response.Headers.Remove("Content-Type");
            }
            response.Headers.Add("Content-Type", "application/json");

            response.WriteStringAsync(json).Wait();

            return response;
        }

        public static HttpResponseData CreateJsonResponse(HttpRequestData request, object responseData, JsonSerializerSettings jsonSerializerSettings = null)
        {
            return CreateJsonResponse(request, responseData, HttpStatusCode.OK, jsonSerializerSettings);
        }

       

    }
}
