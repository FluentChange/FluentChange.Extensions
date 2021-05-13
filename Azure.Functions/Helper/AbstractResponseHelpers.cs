using FluentChange.Extensions.Common.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemNet = System.Net;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public abstract class AbstractResponseHelpers<T, M> where T : new() where M : new()
    {
        protected bool wrapRequestAndResponse;
        protected readonly bool usesMapping;
        private readonly IEntityMapper mapper;
        protected JsonSerializerSettings jsonSettings;
        public AbstractResponseHelpers(IEntityMapper mapper)
        {
            this.mapper = mapper;
            this.wrapRequestAndResponse = false;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public AbstractResponseHelpers<T, M> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public AbstractResponseHelpers<T, M> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }

        protected async Task<T> GetRequestBody(HttpRequest req)
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<M>>(requestBody, jsonSettings);
                    var entityMapped = mapper.MapTo<T>(entityWrapped.Data);
                    return entityMapped;
                }
                else
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<T>>(requestBody, jsonSettings);
                    return entityWrapped.Data;
                }
            }
            else
            {
                if (usesMapping)
                {
                    var entity = JsonConvert.DeserializeObject<M>(requestBody, jsonSettings);
                    var entityMapped = mapper.MapTo<T>(entity);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonConvert.DeserializeObject<T>(requestBody, jsonSettings);
                    return entity;
                }
            }
        }
        protected HttpResponseMessage RespondError(Exception ex)
        {
            if (wrapRequestAndResponse)
            {
                var response = new Response();
                response.Errors.Add(new ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() });
                return ResponseHelper.CreateJsonResponse(response, SystemNet.HttpStatusCode.InternalServerError, jsonSettings);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(null, SystemNet.HttpStatusCode.InternalServerError, jsonSettings);
            }
        }
        protected HttpResponseMessage Respond()
        {
            if (wrapRequestAndResponse)
            {
                var response = new Response();
                return ResponseHelper.CreateJsonResponse(response, jsonSettings);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(null, jsonSettings);
            }
        }

        protected HttpResponseMessage Respond(T result)
        {
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var mappedResult = mapper.MapTo<M>(result);
                    var response = new SingleResponse<M>();
                    response.Result = mappedResult;
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
                else
                {
                    var response = new SingleResponse<T>();
                    response.Result = result;
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }

            }
            else
            {
                if (usesMapping)
                {
                    var mappedResult = mapper.MapTo<M>(result);
                    return ResponseHelper.CreateJsonResponse(mappedResult, jsonSettings);
                }
                else
                {
                    return ResponseHelper.CreateJsonResponse(result, jsonSettings);
                }
            }
        }

        protected HttpResponseMessage Respond(IEnumerable<T> results)
        {
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var mappedResults = mapper.ProjectTo<M>(results.ToList().AsQueryable());
                    var response = new MultiResponse<M>();
                    response.Results = mappedResults.ToList();
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
                else
                {
                    var response = new MultiResponse<T>();
                    response.Results = results.ToList();
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
            }
            else
            {
                if (usesMapping)
                {
                    var mappedResults = mapper.ProjectTo<M>(results.ToList().AsQueryable());
                    return ResponseHelper.CreateJsonResponse(mappedResults, jsonSettings);
                }
                else
                {

                    return ResponseHelper.CreateJsonResponse(results, jsonSettings);
                }
            }
        }

        public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id);


    }
}
