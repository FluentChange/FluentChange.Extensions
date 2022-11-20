using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Azure.Functions.Interfaces;
using System.Net;
using Newtonsoft.Json;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class AllBuilderWithId
    {
        private readonly IServiceProvider provider;
        private Action<HttpRequest, ILogger> contextCreateFunc;
        private JsonSerializerSettings jsonSettings; 

        public AllBuilderWithId(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public AllBuilderWithIdEntity<T, T> ForEntity<T>() where T : class
        {
            var builder = new AllBuilderWithIdEntity<T, T>(provider, contextCreateFunc, jsonSettings);
            return builder;
        }
        public AllBuilderWithIdEntity<T, M> ForEntityWithMapping<T, M>() where T : class where M : class
        {
            var builder = new AllBuilderWithIdEntity<T, M>(provider, contextCreateFunc, jsonSettings);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : class where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public void WithJson(JsonSerializerSettings jsonSettings)
        {
            this.jsonSettings = jsonSettings;
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : class where M : class where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public AllBuilderWithIdEntityServiceF<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete, list);
        }

        public AllBuilderWithId WithContext<C>() where C : IContextCreationServiceNew
        {
            if (contextCreateFunc == null)
            {
                contextCreateFunc = (HttpRequest req, ILogger log) => provider.GetService<C>().Create(req, log);
            }
            else
            {
                var existingCreate = contextCreateFunc;
                // execute multiple context creation services
                contextCreateFunc = (HttpRequest req, ILogger log) =>
                {
                    existingCreate.Invoke(req, log);
                    provider.GetService<C>().Create(req, log);
                };
            }

            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TServiceModel> Use<TServiceModel>() where TServiceModel : class
        {
            var service = provider.GetService<TServiceModel>();
            var mapper = GetMapperService();
            if (mapper == null) throw new Exception("Mapper is missing");

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new AllBuilderWithIdEntityServiceFree<TServiceModel>(service, contextCreateFunc, mapper, jsonSettings);
            return builder;
        }

        public AllBuilderWithIdEntityServiceF<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class where M : class
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, read, update, delete, list);
        }

        private IEntityMapper GetMapperService()
        {
            IEntityMapper mapper = null;
            mapper = provider.GetService<IEntityMapper>();
            if (mapper == null) throw new Exception("Mapper is missing");

            return mapper;
        }

    }


    public class AllBuilderWithIdEntity<T, M> where T : class where M : class
    {
        private readonly IServiceProvider provider;
        //private readonly bool usesMapping;
        private readonly Action<HttpRequest, ILogger> contextCreateFunc;
        private readonly JsonSerializerSettings jsonSettings;
        public AllBuilderWithIdEntity(IServiceProvider provider, Action<HttpRequest, ILogger> contextCreateFunc, JsonSerializerSettings jsonSettings)
        {
            this.provider = provider;
            //this.usesMapping = !(typeof(T).Equals(typeof(M)));
            this.contextCreateFunc = contextCreateFunc;
            this.jsonSettings = jsonSettings;
        }

        public AllBuilderWithIdEntityServiceF<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new AllBuilderWithIdEntityServiceF<T, M, S>(service, contextCreateFunc, mapper, jsonSettings);
            return builder;
        }

        public AllBuilderWithIdEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDLServiceWithId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new AllBuilderWithIdEntityInterfaceService<T, M, S>(service, contextCreateFunc, mapper, jsonSettings);
            return builder;
        }
        private IEntityMapper GetMapperService()
        {
            IEntityMapper mapper = null;
            mapper = provider.GetService<IEntityMapper>();
            if (mapper == null) throw new Exception("Mapper is missing");

            return mapper;
        }
    }



    public class AllBuilderWithIdEntityInterfaceService<T, M, S> where S : class, ICRUDLServiceWithId<T> where M : class where T : class
    {
        private readonly AllBuilderWithIdEntityServiceF<T, M, S> internalBuilder;
        private readonly Action<HttpRequest, ILogger> contextCreateFunc;
        public AllBuilderWithIdEntityInterfaceService(S service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.contextCreateFunc = contextCreateFunc;

            internalBuilder = new AllBuilderWithIdEntityServiceF<T, M, S>(service, contextCreateFunc, mapper, jsonSettings);
            internalBuilder.OnPost(s => s.Create).OnGetWithId(s => s.Read).OnPut(s => s.Update).OnDeleteWithId(s => s.Delete).OnGet(s => s.List);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log);
        }
    }




    public class AllBuilderWithIdEntityServiceFree<TService> : AbstractResponseHelpersWithHandleIdNew where TService : class
    {
        private readonly TService service;
        private readonly Action<HttpRequest, ILogger> contextCreateFunc;

        private bool unwrap = false;
        private bool wrapout = false;
        public AllBuilderWithIdEntityServiceFree(TService service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings) : base(mapper, jsonSettings)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
            this.contextCreateFunc = contextCreateFunc;
        }

        private Func<TService, HttpRequest, Task<HttpResponseMessage>> getFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> getWithIdFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> postFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> putFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> deleteFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> deleteWithIdFunc;

        public AllBuilderWithIdEntityServiceFree<TService> UnwrapRequest()
        {
            unwrap = true;
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> NotUnwrapRequest()
        {
            unwrap = false;
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> WrapResponse()
        {
            wrapout = true;
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> NotWrapResponse()
        {
            wrapout = true;
            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TService> OnPost<Model>(Func<TService, Func<Model>> predicate) where Model : class
        {
            // only model outgoing
            // no need for mapping ingoing and outgoing
            MakePostFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPost<Model>(Func<TService, Func<Model, Model>> predicate) where Model : class
        {
            // same model in and outgoing
            // no need for mapping ingoing and outgoing
            MakePostFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPost<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            // no need for mapping ingoing and outgoing
            MakePostFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPost<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // no need for mapping ingoing
            MakePostFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPost<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // all possible combination
            MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }


        public AllBuilderWithIdEntityServiceFree<TService> OnGetWithId<Model>(Func<TService, Func<Guid, Model>> predicate) where Model : class
        {
            // no need for mapping outgoing
            MakeGetWithIdFunc<Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnGetWithId<Model>(Func<TService, Func<Model>> predicate) where Model : class
        {
            // no need for mapping outgoing
            MakeGetWithIdFunc<Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnGetWithId<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate) where OutgoingModel : class where OutgoingServiceModel : class
        {
            // with mapping outgoing
            MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnGetWithId<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate) where OutgoingModel : class where OutgoingServiceModel : class
        {
            // with mapping outgoing
            MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TService> OnPut<Model>(Func<TService, Func<Model, Model>> predicate) where Model : class
        {
            // same model in and outgoing
            // no need for mapping ingoing and outgoing
            MakePutFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPut<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            // no need for mapping ingoing and outgoing
            MakePutFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPut<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // no need for mapping ingoing
            MakePutFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnPut<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // all possible combination
            MakePutFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }



        public AllBuilderWithIdEntityServiceFree<TService> OnDeleteWithId(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFuncWithId(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnDelete(Func<TService, Action> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TService> OnGet<OutgoingModel>(Func<TService, Func<OutgoingModel>> predicate) where OutgoingModel : class
        {
            MakeGetFunc<OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> OnGet<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate) where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakeGetFuncList<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        private static Guid GetId(HttpRequest req)
        {
            if (!req.RouteValues.ContainsKey("id")) throw new ArgumentNullException("RouteValues do not contain id");
            var idValue = req.RouteValues["id"].ToString();
            if (String.IsNullOrEmpty(idValue)) throw new ArgumentNullException();
            var idGuid = Guid.Parse(idValue);
            return idGuid;
        }

        #region MakeFunction....
        protected void MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
            where IngoingModel : class
            where IngoingServiceModel : class
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequest req) =>
            {
                IngoingServiceModel createData = await EasyGetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke();

                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultCreated, wrapout);

            };
        }
        protected void MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
           where IngoingModel : class
           where IngoingServiceModel : class
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequest req) =>
            {
                IngoingServiceModel createData = await EasyGetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke(createData);

                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultCreated, wrapout);

            };
        }
        protected void MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            getWithIdFunc = async (TService service, HttpRequest req) =>
            {
                var idGuid = GetId(req);

                var resultRead = predicate.Invoke(service).Invoke(idGuid);
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultRead, wrapout);

            };
        }

        protected void MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
         where OutgoingServiceModel : class
         where OutgoingModel : class
        {
            getWithIdFunc = async (TService service, HttpRequest req) =>
            {
                var resultRead = predicate.Invoke(service).Invoke();
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultRead, wrapout);

            };
        }

        protected void MakePutFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
         where IngoingModel : class
         where IngoingServiceModel : class
         where OutgoingServiceModel : class
         where OutgoingModel : class
        {
            putFunc = async (TService service, HttpRequest req) =>
            {
                IngoingServiceModel updateData = await EasyGetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (updateData == null) throw new ArgumentNullException();
                var resultUpdated = predicate.Invoke(service).Invoke(updateData);

                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultUpdated, wrapout);

            };
        }
        protected void MakeDeleteFuncWithId(Func<TService, Action<Guid>> predicate)
        {
            deleteWithIdFunc = async (TService service, HttpRequest req) =>
            {
                var idGuid = GetId(req);

                predicate.Invoke(service).Invoke(idGuid);
                return EasyRespondEmpty(wrapout);

            };
        }
        protected void MakeDeleteFunc(Func<TService, Action> predicate)
        {
            deleteFunc = async (TService service, HttpRequest req) =>
            {
                predicate.Invoke(service).Invoke();
                return EasyRespondEmpty(wrapout);

            };
        }


        protected void MakeGetFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            getFunc = async (TService service, HttpRequest req) =>
            {
                var listResult = predicate.Invoke(service).Invoke();
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(listResult, wrapout);

            };
        }
        protected void MakeGetFuncList<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate)
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            getFunc = async (TService service, HttpRequest req) =>
            {
                var listResult = predicate.Invoke(service).Invoke();
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(listResult, wrapout);

            };
        }

        #endregion

        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            log.LogInformation("AllNuilder handle function " + req.Method.ToUpper() + " " + typeof(TService).Name + "/" + typeof(TService).Name);

            if (contextCreateFunc != null) contextCreateFunc.Invoke(req, log);

            try
            {
                if (req.RouteValues.ContainsKey("id"))
                {
                    if (req.Method == "GET" && getWithIdFunc != null)
                    {
                        return await getWithIdFunc.Invoke(service, req);
                    }
                    if (req.Method == "DELETE" && deleteWithIdFunc != null)
                    {
                        return await deleteWithIdFunc.Invoke(service, req);
                    }
                }
                else
                {
                    if (req.Method == "GET" && getFunc != null)
                    {
                        return await getFunc.Invoke(service, req);
                    }
                    if (req.Method == "DELETE" && deleteFunc != null)
                    {
                        return await deleteFunc.Invoke(service, req);
                    }
                }
              
             
                if (req.Method == "POST" && postFunc != null)
                {
                    return await postFunc.Invoke(service, req);
                }
                if (req.Method == "PUT" && putFunc != null)
                {
                    return await putFunc.Invoke(service, req);
                }
               
                

                return EasyRespondError(new NotImplementedException(req.Method + " method is not implemented in function"), wrapout, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return EasyRespondError(ex, wrapout);
            }
        }
    }

    public class AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> : AllBuilderWithIdEntityServiceFree<TService> where TService : class where ServiceModel : class where ApiModel : class
    {


        public AllBuilderWithIdEntityServiceF(TService service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings) : base(service, contextCreateFunc, mapper, jsonSettings)
        {

        }

        public new AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> UnwrapRequest()
        {
            base.UnwrapRequest();
            return this;
        }
        public new AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> WrapResponse()
        {
            base.WrapResponse();
            return this;
        }

        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> With(Func<TService, Func<ServiceModel, ServiceModel>> create, Func<TService, Func<Guid, ServiceModel>> read, Func<TService, Func<ServiceModel, ServiceModel>> update, Func<TService, Action<Guid>> delete, Func<TService, Func<IEnumerable<ServiceModel>>> list)
        {
            MakePostFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(create);
            MakeGetWithIdFunc<ServiceModel, ApiModel>(read);
            MakePutFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(update);
            MakeDeleteFuncWithId(delete);
            MakeGetFuncList<ServiceModel, ApiModel>(list);

            return this;
        }

        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnPost(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakePostFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnGet(Func<TService, Func<ServiceModel>> predicate)
        {
            MakeGetFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnGetWithId(Func<TService, Func<Guid, ServiceModel>> predicate)
        {
            MakeGetWithIdFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnPut(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakePutFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }

        public new AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnDelete(Func<TService, Action> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }
        public new AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnDeleteWithId(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFuncWithId(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> OnGet(Func<TService, Func<IEnumerable<ServiceModel>>> predicate)
        {
            MakeGetFuncList<ServiceModel, ApiModel>(predicate);
            return this;
        }


    }
}
