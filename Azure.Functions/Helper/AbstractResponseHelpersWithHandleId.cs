using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemNet = System.Net;

namespace FluentChange.Extensions.Azure.Functions.Helper
{

    public abstract class AbstractResponseHelpersNew
    {
        private readonly IEntityMapper mapper;
        protected JsonSerializerSettings jsonSettings;
        public AbstractResponseHelpersNew(IEntityMapper mapper, JsonSerializerSettings jsonSettings)
        {
            this.mapper = mapper;
        }

        public AbstractResponseHelpersNew WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        protected async Task<TServiceModel> GetRequestBody<TServiceModel, TOutputModel>(HttpRequest req, bool unwrapRequest) where TServiceModel : class where TOutputModel : class
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



        public HttpResponseMessage RespondJson(object result, HttpStatusCode code = HttpStatusCode.OK)
        {
            return ResponseHelper.CreateJsonResponse(result, code, jsonSettings);
        }
        public HttpResponseMessage RespondWrapped<TModel>(TModel result) where TModel : class
        {
            var wrappedResponse = new DataResponse<TModel>();
            wrappedResponse.Data = result;
            return RespondJson(wrappedResponse);
        }
        public HttpResponseMessage RespondWrappedList<TServiceModel>(IEnumerable<TServiceModel> results) where TServiceModel : class
        {
            var wrappedResponse = new DataResponse<IEnumerable<TServiceModel>>();
            wrappedResponse.Data = results.ToList();
            return RespondJson(wrappedResponse);
        }
        public HttpResponseMessage RespondMapped<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            var mappedResult = mapper.MapTo<TOutputModel>(result);
            return RespondJson(mappedResult);
        }
        public HttpResponseMessage RespondMappedList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondJson(mappedResults);
        }
        public HttpResponseMessage RespondMappedAndWrapped<TServiceModel, TOutputModel>(TServiceModel result) where TServiceModel : class where TOutputModel : class
        {
            //if (IsGenericList(typeof(TServiceModel)))
            //{

            //    return RespondMappedAndWrappedList<TServiceModel, TOutputModel>(result);
            //}
            var mappedResult = mapper.MapTo<TOutputModel>(result);           
            return RespondWrapped(mappedResult);
        }
        public HttpResponseMessage RespondMappedAndWrappedList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results) where TServiceModel : class where TOutputModel : class
        {
            var mappedResults = mapper.ProjectTo<TOutputModel>(results.ToList().AsQueryable());
            return RespondWrappedList(mappedResults);
        }




        public HttpResponseMessage RespondError(Exception ex, bool wrapResponse, SystemNet.HttpStatusCode code = SystemNet.HttpStatusCode.InternalServerError)
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
        public HttpResponseMessage RespondEmpty(bool wrapResponse)
        {
            if (wrapResponse)
            {
                var response = new Response();
                return RespondJson(response);
            }
            else
            {
                return RespondJson(null);
            }
        }

        protected HttpResponseMessage Respond<TServiceModel, TOutputModel>(TServiceModel result, bool wrapResponse) where TServiceModel : class where TOutputModel : class
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

        protected HttpResponseMessage RespondList<TServiceModel, TOutputModel>(IEnumerable<TServiceModel> results, bool wrapResponse) where TServiceModel : class where TOutputModel : class
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
