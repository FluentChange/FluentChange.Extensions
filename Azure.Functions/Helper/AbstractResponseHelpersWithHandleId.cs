using FluentChange.Extensions.Common.Models.Rest;
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

    public abstract class AbstractResponseHelpers<TServiceModel, TOutputModel> where TServiceModel : class where TOutputModel : class
    {
        protected bool wrapRequestAndResponse;
        protected readonly bool usesMapping;
        private readonly IEntityMapper mapper;
        protected JsonSerializerSettings jsonSettings;
        public AbstractResponseHelpers(IEntityMapper mapper)
        {
            this.mapper = mapper;
            this.wrapRequestAndResponse = false;
            this.usesMapping = !(typeof(TServiceModel).Equals(typeof(TOutputModel)));
        }

    

        protected async Task<TServiceModel> GetRequestBody(HttpRequest req)
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<TOutputModel>>(requestBody, jsonSettings);
                    var entityMapped = mapper.MapTo<TServiceModel>(entityWrapped.Data);
                    return entityMapped;
                }
                else
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<TServiceModel>>(requestBody, jsonSettings);
                    return entityWrapped.Data;
                }
            }
            else
            {
                if (usesMapping)
                {
                    var entity = JsonConvert.DeserializeObject<TOutputModel>(requestBody, jsonSettings);
                    var entityMapped = mapper.MapTo<TServiceModel>(entity);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonConvert.DeserializeObject<TServiceModel>(requestBody, jsonSettings);
                    return entity;
                }
            }
        }
      
        protected HttpResponseMessage RespondError(Exception ex)
        {
            var errorInfo = new Common.Models.ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() };
            if (wrapRequestAndResponse)
            {
                var response = new Response();
                response.Errors.Add(errorInfo);
                return ResponseHelper.CreateJsonResponse(response, SystemNet.HttpStatusCode.InternalServerError, jsonSettings);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(errorInfo, SystemNet.HttpStatusCode.InternalServerError, jsonSettings);
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
        protected HttpResponseMessage Respond(TServiceModel result)
        {
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var mappedResult = mapper.MapTo<TOutputModel>(result);
                    var response = new NewResponse<TOutputModel>();
                    response.Data = mappedResult;
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
                else
                {
                    var response = new NewResponse<TServiceModel>();
                    response.Data = result;
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }

            }
            else
            {
                if (usesMapping)
                {
                    var mappedResult = mapper.MapTo<TOutputModel>(result);
                    return ResponseHelper.CreateJsonResponse(mappedResult, jsonSettings);
                }
                else
                {
                    return ResponseHelper.CreateJsonResponse(result, jsonSettings);
                }
            }
        }
        protected HttpResponseMessage Respond(IEnumerable<TServiceModel> results)
        {
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
                    var response = new NewResponse<IEnumerable<TOutputModel>>();
                    response.Data = mappedResults.ToList();
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
                else
                {
                    var response = new NewResponse<IEnumerable<TServiceModel>>();
                    response.Data = results.ToList();
                    return ResponseHelper.CreateJsonResponse(response, jsonSettings);
                }
            }
            else
            {
                if (usesMapping)
                {
                    var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
                    return ResponseHelper.CreateJsonResponse(mappedResults, jsonSettings);
                }
                else
                {

                    return ResponseHelper.CreateJsonResponse(results, jsonSettings);
                }
            }
        }

    }

    public abstract class AbstractResponseHelpersWithHandle<T, M> : AbstractResponseHelpers<T, M> where T : class where M : class
    {
        protected AbstractResponseHelpersWithHandle(IEntityMapper mapper) : base(mapper)
        {
        }

        public AbstractResponseHelpersWithHandle<T, M> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public AbstractResponseHelpersWithHandle<T, M> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }

        public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log);
    }

    public abstract class AbstractResponseHelpersWithHandleId<T, M> : AbstractResponseHelpers<T, M> where T : class where M : class
    {
        protected AbstractResponseHelpersWithHandleId(IEntityMapper mapper) : base(mapper)
        {
        }
        public AbstractResponseHelpersWithHandleId<T, M> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public AbstractResponseHelpersWithHandleId<T, M> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }
        public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id);
    }
}
