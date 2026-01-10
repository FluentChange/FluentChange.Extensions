using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SystemNet = System.Net;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public abstract class AbstractResponseHelpersIsolated
    {
        private readonly IEntityMapper mapper;
        protected JsonSerializerOptions? jsonOptions;

        public AbstractResponseHelpersIsolated(IEntityMapper mapper, JsonSerializerOptions? jsonOptions)
        {
            this.mapper = mapper;
            this.jsonOptions = jsonOptions;
        }

        public AbstractResponseHelpersIsolated WithJson(JsonSerializerOptions options)
        {
            this.jsonOptions = options;
            return this;
        }

        protected async Task<TServiceModel> GetRequestBody<TServiceModel, TOutputModel>(HttpRequest req, bool unwrapRequest) where TServiceModel : class where TOutputModel : class
        {
            if (req.Body == null) throw new ArgumentNullException();
            var mapRequest = (typeof(TServiceModel) != typeof(TOutputModel));

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (unwrapRequest)
            {
                if (mapRequest)
                {
                    var entityWrapped = JsonHelper.Deserialize<SingleRequest<TOutputModel>>(requestBody, jsonOptions);
                    var entityMapped = mapper.MapTo<TServiceModel>(entityWrapped!.Data);
                    return entityMapped;
                }
                else
                {
                    var entityWrapped = JsonHelper.Deserialize<SingleRequest<TServiceModel>>(requestBody, jsonOptions);
                    return entityWrapped!.Data;
                }
            }
            else
            {
                if (mapRequest)
                {
                    var entity = JsonHelper.Deserialize<TOutputModel>(requestBody, jsonOptions);
                    var entityMapped = mapper.MapTo<TServiceModel>(entity!);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonHelper.Deserialize<TServiceModel>(requestBody, jsonOptions);
                    return entity!;
                }
            }
        }

        protected async Task<TServiceModel> GetRequestBody<TServiceModel, TOutputModel>(HttpRequestData req, bool unwrapRequest) where TServiceModel : class where TOutputModel : class
        {
            if (req.Body == null) throw new ArgumentNullException();
            var mapRequest = (typeof(TServiceModel) != typeof(TOutputModel));

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (unwrapRequest)
            {
                if (mapRequest)
                {
                    var entityWrapped = JsonHelper.Deserialize<SingleRequest<TOutputModel>>(requestBody, jsonOptions);
                    var entityMapped = mapper.MapTo<TServiceModel>(entityWrapped!.Data);
                    return entityMapped;
                }
                else
                {
                    var entityWrapped = JsonHelper.Deserialize<SingleRequest<TServiceModel>>(requestBody, jsonOptions);
                    return entityWrapped!.Data;
                }
            }
            else
            {
                if (mapRequest)
                {
                    var entity = JsonHelper.Deserialize<TOutputModel>(requestBody, jsonOptions);
                    var entityMapped = mapper.MapTo<TServiceModel>(entity!);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonHelper.Deserialize<TServiceModel>(requestBody, jsonOptions);
                    return entity!;
                }
            }
        }

        public HttpResponseData RespondEmpty(HttpRequestData req, HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelperIsolated.CreateEmptyResponse(req, code);
        }
        public HttpResponseData RespondJson(HttpRequestData req, object result, HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelperIsolated.CreateJsonResponse(req, result, code, jsonOptions);
        }
        public HttpResponseData RespondWrapped<TModel>(HttpRequestData req, TModel result) where TModel : class
        {
            var wrappedResponse = new DataResponse<TModel>();
            wrappedResponse.Data = result;
            return RespondJson(req, wrappedResponse);
        }

        public HttpResponseData RespondWrappedList<TServiceModel>(HttpRequestData req, IEnumerable<TServiceModel> results) where TServiceModel : class
        {
            var wrappedResponse = new DataResponse<IEnumerable<TServiceModel>>();
            wrappedResponse.Data = results.ToList();
            return RespondJson(req, wrappedResponse);
        }
        public HttpResponseData RespondMapped<TServiceModel, TOutputModel>(HttpRequestData req, TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return RespondJson(req, mappedResult);
        }
        public HttpResponseData RespondMappedList<TServiceModel, TOutputModel>(HttpRequestData req, IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondJson(req, mappedResults);
        }
        public HttpResponseData RespondMappedAndWrapped<TServiceModel, TOutputModel>(HttpRequestData req, TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            //if (IsGenericList(typeof(TServiceModel)))
            //{

            //    return RespondMappedAndWrappedList<TServiceModel, TOutputModel>(result);
            //}
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return RespondWrapped(req, mappedResult);
        }
        public HttpResponseData RespondMappedAndWrappedList<TServiceModel, TOutputModel>(HttpRequestData req, IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondWrappedList(req, mappedResults);
        }


        public HttpResponseData RespondNotFound(HttpRequestData req)
        {
            return RespondError(req, null, false, HttpStatusCode.NotFound);
        }
        public HttpResponseData RespondNotFound(HttpRequestData req, Exception ex, bool wrapResponse)
        {
            return RespondError(req, ex, wrapResponse, HttpStatusCode.NotFound);
        }

        public HttpResponseData RespondError(HttpRequestData req, Exception? ex, bool wrapResponse, SystemNet.HttpStatusCode code = SystemNet.HttpStatusCode.InternalServerError)
        {
            if (ex != null)
            {
                var errorInfo = new Common.Models.ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() };
                if (wrapResponse)
                {
                    var response = new Response();
                    response.Errors.Add(errorInfo);
                    return RespondJson(req, response, code);
                }
                else
                {
                    return RespondJson(req, errorInfo, code);
                }
            }
            else
            {
                return RespondEmpty(req, code);
            }
        }
        public HttpResponseData RespondEmpty(HttpRequestData req, bool wrapResponse)
        {
            if (wrapResponse)
            {
                var response = new Response();
                return RespondJson(req, response);
            }
            else
            {
                return RespondJson(req, null);
            }
        }

        protected HttpResponseData Respond<TServiceModel, TOutputModel>(HttpRequestData req, TServiceModel result, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var typeInput = typeof(TServiceModel);
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));

            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return RespondMappedAndWrapped<TServiceModel, TOutputModel>(req, result);
                }
                else
                {
                    return RespondWrapped(req, result);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return RespondMapped<TServiceModel, TOutputModel>(req, result);
                }
                else
                {
                    return RespondJson(req, result);
                }
            }
        }

        private static bool IsGenericList(Type typeInput)
        {
            var isList = false;
            if (typeInput.IsGenericType)
            {
                var typeInputGeneric = typeInput.GetGenericTypeDefinition();

                var isIEnumerable = typeInputGeneric == typeof(IEnumerable<>);
                var isIQueryable = typeInputGeneric == typeof(IQueryable<>);

                isList = isIEnumerable || isIQueryable;
            }

            return isList;
        }

        protected HttpResponseData RespondList<TServiceModel, TOutputModel>(HttpRequestData req, IEnumerable<TServiceModel> results, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));
            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return RespondMappedAndWrappedList<TServiceModel, TOutputModel>(req, results);
                }
                else
                {
                    return RespondWrappedList(req, results);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return RespondMappedList<TServiceModel, TOutputModel>(req, results);
                }
                else
                {

                    return RespondJson(req, results);
                }
            }
        }
    }

}
