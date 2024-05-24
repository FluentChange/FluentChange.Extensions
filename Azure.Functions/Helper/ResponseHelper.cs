using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

    public static class ResponseHelper
    {
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
