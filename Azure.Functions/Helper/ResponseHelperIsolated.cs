using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public static class ResponseHelperIsolated
    {
        public static HttpResponseData CreateEmptyResponse(HttpRequestData request, HttpStatusCode statuscode)
        {
            var response = request.CreateResponse(statuscode);
            return response;
        }

        public static HttpResponseData CreateJsonResponse(HttpRequestData request, object responseData, HttpStatusCode statuscode, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var json = JsonHelper.Serialize(responseData, jsonSerializerOptions);

            var response = request.CreateResponse(statuscode);

            if (response.Headers.Contains("Content-Type"))
            {
                response.Headers.Remove("Content-Type");
            }
            response.Headers.Add("Content-Type", "application/json");

            response.WriteStringAsync(json).Wait();

            return response;
        }

        public static HttpResponseData CreateJsonResponse(HttpRequestData request, object responseData, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            return CreateJsonResponse(request, responseData, HttpStatusCode.OK, jsonSerializerOptions);
        }
    }
}
