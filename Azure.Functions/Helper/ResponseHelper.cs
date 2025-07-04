using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
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
        public static IActionResult CreateJsonResponse(object response, HttpStatusCode statuscode, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var json = "";
            if (jsonSerializerSettings == null)
            {
                json = JsonConvert.SerializeObject(response);
            }
            else
            {
                json = JsonConvert.SerializeObject(response, jsonSerializerSettings);
            }

            //return new JsonResult(response,settings); // this is currently not working
            //return new HttpResponseMessage(statuscode)
            //{
            //    Content = new StringContent(json, Encoding.UTF8, "application/json")
            //};

            var result = new ContentResult();
            result.Content = json;
            result.ContentType = "application/json";
            result.StatusCode = (int) statuscode;
            return result;
        }

        public static IActionResult CreateJsonResponse(object response, JsonSerializerSettings jsonSerializerSettings = null)
        {
            return CreateJsonResponse(response, HttpStatusCode.OK, jsonSerializerSettings);
        }

       

    }
}
