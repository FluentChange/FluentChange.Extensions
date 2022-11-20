using FluentChange.Extensions.Azure.Functions.Interfaces;
using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

    public abstract class AbstractResponseHelpersNew
    {
        private readonly IEntityMapper mapper;
        protected JsonSerializerSettings jsonSettings;
        public AbstractResponseHelpersNew(IEntityMapper mapper, JsonSerializerSettings jsonSettings)
        {
            this.mapper = mapper;   
        }

        protected async Task<TServiceModel> EasyGetRequestBody<TServiceModel, TOutputModel>(HttpRequest req, bool unwrapRequest) where TServiceModel : class where TOutputModel : class
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();
            var mapRequest = (typeof(TServiceModel) != typeof(TOutputModel));

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (unwrapRequest)
            {
                if (mapRequest)
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
                if (mapRequest)
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



        public HttpResponseMessage JsonRespond(object result, HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelper.CreateJsonResponse(result, code, jsonSettings);
        }

        public HttpResponseMessage WrapRespond<TModel>(TModel result) where TModel : class
        {
            var wrappedResponse = new NewResponse<TModel>();
            wrappedResponse.Data = result;
            return JsonRespond(wrappedResponse);
        }
        public HttpResponseMessage WrapRespondList<TServiceModel>(IEnumerable<TServiceModel> results) where TServiceModel : class
        {
            var wrappedResponse = new NewResponse<IEnumerable<TServiceModel>>();
            wrappedResponse.Data = results.ToList();
            return JsonRespond(wrappedResponse);
        }
        public HttpResponseMessage MapRespond<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return JsonRespond(mappedResult);
        }
        public HttpResponseMessage MapRespond<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return JsonRespond(mappedResults);
        }

        public HttpResponseMessage MapAndWrapRespond<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return WrapRespond(mappedResult);
        }

        public HttpResponseMessage MapAndWrapRespond<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return WrapRespondList(mappedResults);
        }




        protected HttpResponseMessage EasyRespondError(Exception ex, bool wrapResponse, SystemNet.HttpStatusCode code = SystemNet.HttpStatusCode.InternalServerError)
        {
            var errorInfo = new Common.Models.ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() };
            if (wrapResponse)
            {
                var response = new Response();
                response.Errors.Add(errorInfo);
                return JsonRespond(response, code);
            }
            else
            {
                return JsonRespond(errorInfo, code);
            }
        }

        protected HttpResponseMessage EasyRespond<TServiceModel, TOutputModel>(TServiceModel result, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));
            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return MapAndWrapRespond<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return WrapRespond(result);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return MapRespond<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return JsonRespond(result);
                }
            }
        }
        protected HttpResponseMessage EasyRespond<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));
            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return MapAndWrapRespond<TServiceModel, TOutputModel>(results);
                }
                else
                {
                    return WrapRespondList(results);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return MapRespond<TServiceModel, TOutputModel>(results);
                }
                else
                {

                    return JsonRespond(results);
                }
            }
        }
        protected HttpResponseMessage EasyRespondEmpty(bool wrapResponse)
        {
            if (wrapResponse)
            {
                var response = new Response();
                return JsonRespond(response);
            }
            else
            {
                return JsonRespond(null);
            }
        }

    }

    public abstract class AbstractResponseHelpersWithHandle<TServiceModel, TOutputModel> : AbstractResponseHelpers<TServiceModel, TOutputModel> where TServiceModel : class where TOutputModel : class
    {
        protected AbstractResponseHelpersWithHandle(IEntityMapper mapper) : base(mapper)
        {
        }

        public AbstractResponseHelpersWithHandle<TServiceModel, TOutputModel> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public AbstractResponseHelpersWithHandle<TServiceModel, TOutputModel> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }

        public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log);
    }

    public abstract class AbstractResponseHelpersWithHandleId<TServiceModel, TOutputModel> : AbstractResponseHelpers<TServiceModel, TOutputModel> where TServiceModel : class where TOutputModel : class
    {
        protected AbstractResponseHelpersWithHandleId(IEntityMapper mapper) : base(mapper)
        {
        }
        public AbstractResponseHelpersWithHandleId<TServiceModel, TOutputModel> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public AbstractResponseHelpersWithHandleId<TServiceModel, TOutputModel> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }
        public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id);
    }

    public abstract class AbstractResponseHelpersWithHandleIdNew : AbstractResponseHelpersNew
    {
        protected AbstractResponseHelpersWithHandleIdNew(IEntityMapper mapper, JsonSerializerSettings jsonSettings) : base(mapper, jsonSettings)
        {
        }
        public AbstractResponseHelpersWithHandleIdNew WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        //public AbstractResponseHelpersWithHandleIdNew WrapRequestAndResponse()
        //{
        //    wrapRequestAndResponse = true;
        //    return this;
        //}
        //public abstract Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id);
    }
}
