using FluentChange.Extensions.Common.Models.Rest;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public abstract class AbstractInternalRestClient : IRestClient
    {
        public AbstractInternalRestClient()
        {

        }

        // The GET method requests a representation of the specified resource. Requests using GET should only retrieve data and should not contain a request content.
        protected abstract Task<HttpResponseMessage> GetImplementation(string route, Dictionary<string, object> parameters);
        public async Task<T> Get<T>(string route, Dictionary<string, object> parameters = null)
        {
            var response = await GetImplementation(route, parameters);
            return await HandleContentOrError<T>(response);
        }

        // GetStream - for downloading files as streams
        public async Task<Stream> GetStream(string route, Dictionary<string, object> parameters = null)
        {
            var response = await GetImplementation(route, parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else
            {
                throw await HandleError(response);
            }
        }

        // The HEAD method asks for a response identical to a GET request, but without a response body.
        protected abstract Task<HttpResponseMessage> HeadImplementation(string route, Dictionary<string, object> parameters);
        public async Task<T> Head<T>(string route, Dictionary<string, object> parameters = null)
        {
            var response = await HeadImplementation(route, parameters);
            return await HandleContentOrError<T>(response);
        }


        // The POST method submits an entity to the specified resource, often causing a change in state or side effects on the server.
        protected abstract Task<HttpResponseMessage> PostImplementation(string route, object content, Dictionary<string, object> parameters);
        protected async Task<T> PostInternal<T>(string route, object content, Dictionary<string, object> parameters = null)
        {
            var response = await PostImplementation(route, content, parameters);
            return await HandleContentOrError<T>(response);
        }
        public async Task<T> Post<T>(string route, object content, Dictionary<string, object> parameters = null)
        {
            var response = await PostInternal<T>(route, content, parameters);
            return response;
        }
        public async Task<T> PostFile<T>(string route, string filePath, Dictionary<string, object> parameters = null)
        {
            var file = new FileInfo(filePath);
            return await PostFile<T>(route, file.OpenRead(), file.Name, parameters);
        }

        public async Task<T> PostFile<T>(string route, Stream fileStream, string fileName, Dictionary<string, object> parameters = null)
        {
            var content = new MultipartFormDataContent();

            string mimeType = MimeTypes.GetMimeType(fileName);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            content.Add(fileContent, "file", fileName);
            return await PostInternal<T>(route, content, parameters);
        }


        // The PUT method replaces all current representations of the target resource with the request content.
        protected abstract Task<HttpResponseMessage> PutImplementation(string route, object content, Dictionary<string, object> parameters);
        public async Task<T> Put<T>(string route, object content, Dictionary<string, object> parameters = null)
        {
            var response = await PutImplementation(route, content, parameters);
            return await HandleContentOrError<T>(response);
        }

        // The DELETE method deletes the specified resource.
        protected abstract Task<HttpResponseMessage> DeleteImplementation(string route, Dictionary<string, object> parameters);
        public async Task<T> Delete<T>(string route, Dictionary<string, object> parameters = null)
        {
            var response = await DeleteImplementation(route, parameters);
            return await HandleContentOrError<T>(response);
        }

        // The CONNECT method establishes a tunnel to the server identified by the target resource
        protected abstract Task<HttpResponseMessage> ConnectImplementation(string route, Dictionary<string, object> parameters);
        public async Task<T> Connect<T>(string route, Dictionary<string, object> parameters = null)
        {
            var response = await ConnectImplementation(route, parameters);
            return await HandleContentOrError<T>(response);
        }

        // The OPTIONS method describes the communication options for the target resource.
        protected abstract Task<HttpResponseMessage> OptionsImplementation(string route, Dictionary<string, object> parameters);
        public async Task<T> Options<T>(string route, Dictionary<string, object> parameters = null)
        {
            var response = await OptionsImplementation(route, parameters);
            return await HandleContentOrError<T>(response);
        }

        // The TRACE method performs a message loop-back test along the path to the target resource.
        protected abstract Task<HttpResponseMessage> TraceImplementation(string route, object content, Dictionary<string, object> parameters);
        public async Task<T> Trace<T>(string route, object content, Dictionary<string, object> parameters = null)
        {
            var response = await TraceImplementation(route, content, parameters);
            return await HandleContentOrError<T>(response);
        }


        // The PATCH method applies partial modifications to a resource.
        protected abstract Task<HttpResponseMessage> PatchImplementation(string route, object content, Dictionary<string, object> parameters);
        public async Task<T> Patch<T>(string route, object content, Dictionary<string, object> parameters = null)
        {
            var response = await PatchImplementation(route, content, parameters);
            return await HandleContentOrError<T>(response);
        }


        protected static HttpContent SerializeContentIfNeeded(object requestBody)
        {

            var requestBodyType = requestBody.GetType();
            HttpContent content;
            if (requestBodyType == typeof(MultipartFormDataContent))
            {
                var multiPartContent = (MultipartFormDataContent)requestBody;
                content = multiPartContent;
            }
            else if (requestBodyType == typeof(StringContent))
            {
                content = (StringContent)requestBody;
            }
            else
            {
                content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            }

            return content;
        }



        // HEADER stuff
        public abstract bool ExistHeader(string key);
        public abstract void SetHeader(string key, string value);
        public abstract void RemoveHeader(string key);

        // AUTH stuff

        private string AUTH_KEY = "Authorization";
        public bool IsAuthenticated => ExistHeader(AUTH_KEY);

        public void AuthorizeWithBearer(string token)
        {
            SetHeader(AUTH_KEY, string.Format("Bearer {0}", token));
        }
        public void AuthorizeWithSecret(string secret)
        {
            SetHeader(AUTH_KEY, string.Format("Secret {0}", secret));
        }
        public void DeAuthorize()
        {
            RemoveHeader(AUTH_KEY);
        }

        // HELPER

        private async Task<T> HandleContentOrError<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
            {
                return await HandleContent<T>(response);
            }
            else
            {
                throw await HandleError(response);
            }
        }
        private static async Task<T> HandleContent<T>(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(data);

            return result;
        }
        protected async Task<Exception> HandleError(HttpResponseMessage response)
        {
            var message = response.ReasonPhrase + " (" + ((int)response.StatusCode) + ")" + Environment.NewLine;
            var exceptionData = new Dictionary<string, string>();

            if (response.Content != null)
            {
                try
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Response>(data);

                    if (result != null && result.Errors != null)
                    {
                        var j = 1;
                        foreach (var error in result.Errors)
                        {
                            message += "- Error " + j + ": " + error.Message + Environment.NewLine;
                            j++;
                        }


                        var i = 1;
                        foreach (var error in result.Errors)
                        {
                            exceptionData.Add("Error " + i, error.FullMessage);
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptionData.Add("Reading Response Content", ex.ToString());
                }


            }
            var exception = new Exception(message);
            foreach (var data in exceptionData)
            {
                exception.Data.Add(data.Key, data.Value);
            }
            return exception;
        }

        protected string ReplaceParams(string route, Dictionary<string, object> parameters)
        {
            //if (parameters != null)
            //{
            //    foreach (var param in parameters)
            //    {
            //        if (param.Key.StartsWith("{") && param.Key.EndsWith("}"))
            //        {
            //            route = route.Replace(param.Key, param.Value);
            //        }
            //        else
            //        {
            //            route = route.Replace("{" + param.Key + "}", param.Value);
            //        }
            //    }
            //}

            //return route;
            Dictionary<string, string> dictionary;
            var result = ReplaceParams(route, parameters, out dictionary);
            return result;
        }

        protected string ReplaceParams(string route, Dictionary<string, object> parameters, out Dictionary<string, string> routevalues)
        {
            routevalues = new Dictionary<string, string>();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param.Key.StartsWith("{") && (param.Key.EndsWith("}") || param.Key.EndsWith("?}")))
                    {
                        if (route.Contains(param.Key))
                        {
                            route = route.Replace(param.Key, param.Value.ToString());
                            routevalues.Add(param.Key, param.Value.ToString());
                        }
                    }
                    else
                    {
                        if (route.Contains("{" + param.Key + "}"))
                        {
                            route = route.Replace("{" + param.Key + "}", param.Value.ToString());
                            routevalues.Add(param.Key, param.Value.ToString());
                        }
                        if (route.Contains("{" + param.Key + "?}"))
                        {
                            route = route.Replace("{" + param.Key + "?}", param.Value.ToString());
                            routevalues.Add(param.Key, param.Value.ToString());
                        }
                        if (route.Contains("{*" + param.Key + "}"))
                        {
                            route = route.Replace("{*" + param.Key + "}", param.Value.ToString());
                            routevalues.Add(param.Key, param.Value.ToString());
                        }
                    }
                }
            }

            return route;
        }
    }
}
