﻿using MimeKit;
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


        protected abstract Task<HttpResponseMessage> GetImplementation(string route, Dictionary<string, string> parameters);
        public async Task<T> Get<T>(string route, Dictionary<string, string> parameters = null)
        {
            var response = await GetImplementation(route, parameters);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(data);
                return result;
            }
            else
            {
                throw await HandleError(response);
            }
        }

        protected abstract Task<HttpResponseMessage> PostImplementation(string route, HttpContent content, Dictionary<string, string> parameters);
        protected async Task<T> PostInternal<T>(string route, HttpContent content, Dictionary<string, string> parameters = null)
        {
            var response = await PostImplementation(route, content, parameters);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(data);

                return result;
            }
            else
            {
                throw await HandleError(response);
            }
        }
        public async Task<T> Post<T>(string route, object requestBody, Dictionary<string, string> parameters = null)
        {
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await PostInternal<T>(route, content, parameters);
            return response;
        }
        public async Task<T> PostFile<T>(string route, string filePath, Dictionary<string, string> parameters = null)
        {
            var content = new MultipartFormDataContent();

            var file = new FileInfo(filePath);
            string mimeType = MimeTypes.GetMimeType(file.Name);
            var fileContent = new StreamContent(file.OpenRead());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            content.Add(fileContent, "file", file.Name);
            return await PostInternal<T>(route, content, parameters);
        }

        protected abstract Task<HttpResponseMessage> PutImplementation(string route, HttpContent content, Dictionary<string, string> parameters);
        public async Task<T> Put<T>(string route, object requestBody, Dictionary<string, string> parameters = null)
        {
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await PutImplementation(route, content, parameters);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(data);

                return result;
            }
            else
            {
                throw await HandleError(response);
            }
        }

        protected abstract Task<HttpResponseMessage> DeleteImplementation(string route, Dictionary<string, string> parameters);
        public async Task<T> Delete<T>(string route, Dictionary<string, string> parameters = null)
        {
            var response = await DeleteImplementation(route, parameters);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(data);

                return result;
            }
            else
            {
                throw await HandleError(response);
            }
        }


        // AUTH stuff

        private string AUTH_KEY = "Authorization";
        public bool IsAuthenticated => ExistHeader(AUTH_KEY);

        protected abstract bool ExistHeader(string key);
        protected abstract void SetHeader(string key, string value);
        protected abstract void RemoveHeader(string key);
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

        protected async Task<Exception> HandleError(HttpResponseMessage response)
        {
            var message = response.ReasonPhrase + " (" + ((int)response.StatusCode) + ")" + Environment.NewLine;
            var exception = new Exception(message);

            if (response.Content != null)
            {
                try
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Response>(data);


                    var j = 1;
                    foreach (var error in result.Errors)
                    {
                        message += "- Error " + j + ": " + error.Message + Environment.NewLine;
                        j++;
                    }


                    var i = 1;
                    foreach (var error in result.Errors)
                    {
                        exception.Data.Add("Error " + i, error.FullMessage);
                        i++;
                    }

                }
                catch (Exception ex)
                {
                    exception.Data.Add("Reading Response Content", ex.ToString());
                }


            }

            return exception;
        }

        protected string ReplaceParams(string route, Dictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param.Key.StartsWith("{") && param.Key.EndsWith("}"))
                    {
                        route = route.Replace(param.Key, param.Value);
                    }
                    else
                    {
                        route = route.Replace("{" + param.Key + "}", param.Value);
                    }
                }
            }

            return route;
        }
    }
}