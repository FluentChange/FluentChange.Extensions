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

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class AllBuilderWithId
    {
        private readonly IServiceProvider provider;
        private Action<HttpRequest, ILogger> contextCreateFunc;

        public AllBuilderWithId(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public AllBuilderWithIdEntity<T, T> ForEntity<T>() where T : class
        {
            var builder = new AllBuilderWithIdEntity<T, T>(provider, contextCreateFunc);
            return builder;
        }
        public AllBuilderWithIdEntity<T, M> ForEntityWithMapping<T, M>() where T : class where M : class
        {
            var builder = new AllBuilderWithIdEntity<T, M>(provider, contextCreateFunc);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : class where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
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

            var builder = new AllBuilderWithIdEntityServiceFree<TServiceModel>(service, contextCreateFunc, mapper);
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
        public AllBuilderWithIdEntity(IServiceProvider provider, Action<HttpRequest, ILogger> contextCreateFunc)
        {
            this.provider = provider;
            //this.usesMapping = !(typeof(T).Equals(typeof(M)));
            this.contextCreateFunc = contextCreateFunc;
        }

        public AllBuilderWithIdEntityServiceF<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new AllBuilderWithIdEntityServiceF<T, M, S>(service, contextCreateFunc, mapper);
            return builder;
        }

        public AllBuilderWithIdEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDLServiceWithId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new AllBuilderWithIdEntityInterfaceService<T, M, S>(service, contextCreateFunc, mapper);
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
        public AllBuilderWithIdEntityInterfaceService(S service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.contextCreateFunc = contextCreateFunc;

            internalBuilder = new AllBuilderWithIdEntityServiceF<T, M, S>(service, contextCreateFunc, mapper);
            internalBuilder.Create(s => s.Create).Read(s => s.Read).Update(s => s.Update).Delete(s => s.Delete).List(s => s.List);
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

        private bool unwrap = true;
        private bool wrapout = true;
        public AllBuilderWithIdEntityServiceFree(TService service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
            this.contextCreateFunc = contextCreateFunc;
        }

        private Func<TService, HttpRequest, Task<HttpResponseMessage>> createFunc;
        //private Func<TService, HttpRequest, string, Task<HttpResponseMessage>> readFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> readFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> updateFunc;
        //private Func<TService, HttpRequest, string, Task<HttpResponseMessage>> deleteFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> deleteFunc;
        private Func<TService, HttpRequest, Task<HttpResponseMessage>> listFunc;

        public AllBuilderWithIdEntityServiceFree<TService> WrapRequestAndResponse()
        {
            unwrap = true;
            wrapout = true;
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> NotWrapRequestAndResponse()
        {
            unwrap = false;
            wrapout = false;
            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TService> UnwrapRequest()
        {
            unwrap = true;
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> WrapResponse()
        {
            wrapout = true;
            return this;
        }

        public AllBuilderWithIdEntityServiceFree<TService> Create<Model>(Func<TService, Func<Model, Model>> predicate) where Model : class
        {
            // same model in and outgoing
            // no need for mapping ingoing and outgoing
            MakeCreateFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Create<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            // no need for mapping ingoing and outgoing
            MakeCreateFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Create<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // no need for mapping ingoing
            MakeCreateFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Create<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // all possible combination
            MakeCreateFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }


        public AllBuilderWithIdEntityServiceFree<TService> Read<Model>(Func<TService, Func<Guid, Model>> predicate) where Model : class
        {
            // no need for mapping outgoing
            MakeReadFunc<Model, Model>(predicate);
            MakeReadFuncNoId<Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Read<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate) where OutgoingModel : class where OutgoingServiceModel : class
        {
            // with mapping outgoing
            MakeReadFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            MakeReadFuncNoId<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }


        public AllBuilderWithIdEntityServiceFree<TService> Update<Model>(Func<TService, Func<Model, Model>> predicate) where Model : class
        {
            // same model in and outgoing
            // no need for mapping ingoing and outgoing
            MakeUpdateFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Update<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            // no need for mapping ingoing and outgoing
            MakeUpdateFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Update<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // no need for mapping ingoing
            MakeUpdateFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceFree<TService> Update<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            // all possible combination
            MakeUpdateFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }



        public AllBuilderWithIdEntityServiceFree<TService> Delete(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }



        public AllBuilderWithIdEntityServiceFree<TService> List<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate) where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakeListFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }

        #region MakeFunction....
        protected void MakeCreateFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
           where IngoingModel : class
           where IngoingServiceModel : class
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            createFunc = async (TService service, HttpRequest req) =>
            {
                IngoingServiceModel createData = await EasyGetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke(createData);

                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultCreated, wrapout);

            };
        }
        protected void MakeReadFuncNoId<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            readFunc = async (TService service, HttpRequest req) =>
            {
                //if (String.IsNullOrEmpty(id)) throw new ArgumentNullException();
                //var idGuid = Guid.Parse(id);

                var idGuid = GetId(req);

                var resultRead = predicate.Invoke(service).Invoke(idGuid);
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultRead, wrapout);

            };
        }

        private static Guid GetId(HttpRequest req)
        {
            if (!req.RouteValues.ContainsKey("id")) throw new ArgumentNullException("RouteValues do not contain id");
            var idValue = req.RouteValues["id"].ToString();
            if (String.IsNullOrEmpty(idValue)) throw new ArgumentNullException();
            var idGuid = Guid.Parse(idValue);
            return idGuid;
        }

        protected void MakeReadFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
          where OutgoingServiceModel : class
          where OutgoingModel : class
        {
            //readFunc = async (TService service, HttpRequest req, string id) =>
            //{

            //    //if (String.IsNullOrEmpty(id)) throw new ArgumentNullException();
            //    //var idGuid = Guid.Parse(id);

            //    if (!req.RouteValues.ContainsKey("id")) throw new ArgumentNullException("RouteValues do not contain id");
            //    var idValue = req.RouteValues["id"].ToString();
            //    if (String.IsNullOrEmpty(idValue)) throw new ArgumentNullException();
            //    var idGuid = Guid.Parse(idValue);

            //    var resultRead = predicate.Invoke(service).Invoke(idGuid);
            //    return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultRead, wrapout);

            //};
        }
        protected void MakeUpdateFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
         where IngoingModel : class
         where IngoingServiceModel : class
         where OutgoingServiceModel : class
         where OutgoingModel : class
        {
            updateFunc = async (TService service, HttpRequest req) =>
            {
                IngoingServiceModel updateData = await EasyGetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (updateData == null) throw new ArgumentNullException();
                var resultUpdated = predicate.Invoke(service).Invoke(updateData);

                return EasyRespond<OutgoingServiceModel, OutgoingModel>(resultUpdated, wrapout);

            };
        }
        protected void MakeDeleteFunc(Func<TService, Action<Guid>> predicate)
        {
            deleteFunc = async (TService service, HttpRequest req) =>
            {
                //if (String.IsNullOrEmpty(id)) throw new ArgumentNullException();
                //var idGuid = Guid.Parse(id);

                var idGuid = GetId(req);

                predicate.Invoke(service).Invoke(idGuid);
                return EasyRespondEmpty(wrapout);

            };
        }

        protected void MakeListFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate)
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            listFunc = async (TService service, HttpRequest req) =>
            {
                var listResult = predicate.Invoke(service).Invoke();
                return EasyRespond<OutgoingServiceModel, OutgoingModel>(listResult, wrapout);

            };
        }

        #endregion
        //public override async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        //{
        //    log.LogInformation("AllNuilder handle function " + req.Method.ToUpper() + " " + typeof(TService).Name + "/" + typeof(TService).Name);

        //    if (contextCreateFunc != null) contextCreateFunc.Invoke(req, log);

        //    try
        //    {
        //        if (req.Method == "GET" && !String.IsNullOrEmpty(id) && readFunc != null)
        //        {
        //            return await readFunc.Invoke(service, req, id);
        //        }
        //        if (req.Method == "GET" && String.IsNullOrEmpty(id) && listFunc != null)
        //        {
        //            return await listFunc.Invoke(service, req);
        //        }
        //        if (req.Method == "POST" && createFunc != null)
        //        {
        //            return await createFunc.Invoke(service, req);
        //        }
        //        if (req.Method == "PUT" && updateFunc != null)
        //        {
        //            return await updateFunc.Invoke(service, req);
        //        }
        //        if (req.Method == "DELETE" && deleteFunc != null)
        //        {
        //            return await deleteFunc.Invoke(service, req, id);
        //        }

        //        return EasyRespondError(new NotImplementedException(req.Method + " method is not implemented in function"), wrapout, HttpStatusCode.BadRequest);
        //    }
        //    catch (Exception ex)
        //    {
        //        return EasyRespondError(ex, wrapout);
        //    }
        //}

        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            log.LogInformation("AllNuilder handle function " + req.Method.ToUpper() + " " + typeof(TService).Name + "/" + typeof(TService).Name);

            if (contextCreateFunc != null) contextCreateFunc.Invoke(req, log);

            try
            {
                if (req.Method == "GET" && req.RouteValues.ContainsKey("id") && readFunc != null)
                {
                    return await readFunc.Invoke(service, req);
                }
                if (req.Method == "GET" && !req.RouteValues.ContainsKey("id") && listFunc != null)
                {
                    return await listFunc.Invoke(service, req);
                }
                if (req.Method == "POST" && createFunc != null)
                {
                    return await createFunc.Invoke(service, req);
                }
                if (req.Method == "PUT" && updateFunc != null)
                {
                    return await updateFunc.Invoke(service, req);
                }
                if (req.Method == "DELETE" && deleteFunc != null)
                {
                    return await deleteFunc.Invoke(service, req);
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


        public AllBuilderWithIdEntityServiceF(TService service, Action<HttpRequest, ILogger> contextCreateFunc, IEntityMapper mapper) : base(service, contextCreateFunc, mapper)
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
            MakeCreateFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(create);
            MakeReadFunc<ServiceModel, ApiModel>(read);
            MakeUpdateFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(update);
            MakeDeleteFunc(delete);
            MakeListFunc<ServiceModel, ApiModel>(list);

            return this;
        }

        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Create(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakeCreateFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        //public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Read(Func<TService, Func<ServiceModel>> predicate)
        //{
        //    MakeReadFunc<ServiceModel, ApiModel>(predicate);
        //    return this;
        //}
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Read(Func<TService, Func<Guid, ServiceModel>> predicate)
        {
            MakeReadFuncNoId<ServiceModel, ApiModel>(predicate);
            MakeReadFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Update(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakeUpdateFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Delete(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }
        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> List(Func<TService, Func<IEnumerable<ServiceModel>>> predicate)
        {
            MakeListFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }

        public AllBuilderWithIdEntityServiceF<ServiceModel, ApiModel, TService> Id(Func<string> id)
        {
            throw new NotImplementedException();
        }
    }
}
