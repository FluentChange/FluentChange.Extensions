#nullable enable
using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SystemNet = System.Net;

namespace FluentChange.Extensions.Azure.Functions.Helper
{

    public abstract class AbstractResponseHelpersNew
    {
        private readonly IEntityMapper mapper;
        protected JsonSerializerOptions? jsonOptions;
        public AbstractResponseHelpersNew(IEntityMapper mapper, JsonSerializerOptions? jsonOptions)
        {
            this.mapper = mapper;
            this.jsonOptions = jsonOptions;
        }

        public AbstractResponseHelpersNew WithJson(JsonSerializerOptions? options)
        {
            this.jsonOptions = options;
            return this;
        }

        protected async Task<TServiceModel> GetRequestBody<TServiceModel, TOutputModel>(HttpRequest req, bool unwrapRequest) where TServiceModel : class where TOutputModel : class
        {
            if (req.Body == null) throw new ArgumentNullException();
            //if (req.Body.Length == 0) throw new ArgumentNullException();
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
                    var entityMapped = mapper.MapTo<TServiceModel>(entity);
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
            //if (req.Body.Length == 0) throw new ArgumentNullException();
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
                    var entityMapped = mapper.MapTo<TServiceModel>(entity);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonHelper.Deserialize<TServiceModel>(requestBody, jsonOptions);
                    return entity!;
                }
            }
        }

        public IActionResult RespondEmpty(HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelper.CreateEmptyResponse(code);
        }
        public IActionResult RespondJson(object result, HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelper.CreateJsonResponse(result, code, jsonOptions);
        }
        public IActionResult RespondWrapped<TModel>(TModel result) where TModel : class
        {
            var wrappedResponse = new DataResponse<TModel>();
            wrappedResponse.Data = result;
            return RespondJson(wrappedResponse);
        }
        public IActionResult RespondWrappedIsolated<TModel>(TModel result) where TModel : class
        {
            var wrappedResponse = new DataResponse<TModel>();
            wrappedResponse.Data = result;
            return RespondJson(wrappedResponse);
        }
        public IActionResult RespondWrappedList<TServiceModel>(IEnumerable<TServiceModel> results) where TServiceModel : class
        {
            var wrappedResponse = new DataResponse<IEnumerable<TServiceModel>>();
            wrappedResponse.Data = results.ToList();
            return RespondJson(wrappedResponse);
        }
        public IActionResult RespondMapped<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return RespondJson(mappedResult);
        }
        public IActionResult RespondMappedList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondJson(mappedResults);
        }
        public IActionResult RespondMappedAndWrapped<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            //if (IsGenericList(typeof(TServiceModel)))
            //{

            //    return RespondMappedAndWrappedList<TServiceModel, TOutputModel>(result);
            //}
            var mappedResult = mapper.MapTo<TOutputModel>(result);           
            return RespondWrapped(mappedResult);
        }
        public IActionResult RespondMappedAndWrappedList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondWrappedList(mappedResults);
        }


        public IActionResult RespondNotFound()
        {
            return RespondError(null, false, HttpStatusCode.NotFound);
        }
        public IActionResult RespondNotFound(Exception ex, bool wrapResponse)
        {
            return RespondError(ex, wrapResponse, HttpStatusCode.NotFound);
        }

        public IActionResult RespondError(Exception? ex, bool wrapResponse, SystemNet.HttpStatusCode code = SystemNet.HttpStatusCode.InternalServerError)
        {
            if (ex != null)
            {
                var errorInfo = new Common.Models.ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() };
                if (wrapResponse)
                {
                    var response = new Response();
                    response.Errors.Add(errorInfo);
                    return RespondJson(response, code);
                }
                else
                {
                    return RespondJson(errorInfo, code);
                }
            }
            else
            {
                return RespondEmpty(code);
            }
        }
        public IActionResult RespondEmpty(bool wrapResponse)
        {
            if (wrapResponse)
            {
                var response = new Response();
                return RespondJson(response);
            }
            else
            {
                return RespondJson(null!);
            }
        }

        protected IActionResult Respond<TServiceModel, TOutputModel>(TServiceModel result, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var typeInput = typeof(TServiceModel);
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));

            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return RespondMappedAndWrapped<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return RespondWrapped(result);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return RespondMapped<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return RespondJson(result);
                }
            }
        }
        protected IActionResult RespondIsolated<TServiceModel, TOutputModel>(TServiceModel result, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var typeInput = typeof(TServiceModel);
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));

            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return RespondMappedAndWrapped<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return RespondWrapped(result);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return RespondMapped<TServiceModel, TOutputModel>(result);
                }
                else
                {
                    return RespondJson(result);
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

        protected IActionResult RespondList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results, bool wrapResponse) where TServiceModel : class where TOutputModel : class
        {
            var mapResponse = (typeof(TServiceModel) != typeof(TOutputModel));
            if (wrapResponse)
            {
                if (mapResponse)
                {
                    return RespondMappedAndWrappedList<TServiceModel, TOutputModel>(results);
                }
                else
                {
                    return RespondWrappedList(results);
                }
            }
            else
            {
                if (mapResponse)
                {
                    return RespondMappedList<TServiceModel, TOutputModel>(results);
                }
                else
                {

                    return RespondJson(results);
                }
            }
        }
    }

}
