using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public static class ResponseHelper
    {
        public static IActionResult CreateEmptyResponse(HttpStatusCode statuscode)
        {
            var result = new StatusCodeResult((int)statuscode);
            return result;
        }

        public static IActionResult CreateJsonResponse(object response, HttpStatusCode statuscode, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var json = JsonHelper.Serialize(response, jsonSerializerOptions);

            var result = new ContentResult();
            result.Content = json;
            result.ContentType = "application/json";
            result.StatusCode = (int)statuscode;
            return result;
        }

        public static IActionResult CreateJsonResponse(object response, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            return CreateJsonResponse(response, HttpStatusCode.OK, jsonSerializerOptions);
        }
    }
}
