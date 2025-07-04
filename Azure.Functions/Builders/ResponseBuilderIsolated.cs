using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Azure.Functions.Interfaces;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class ResponseBuilderIsolated
    {
        private readonly IServiceProvider provider;
        private Func<ILogger, Task> contextCreateFunc;
        private JsonSerializerSettings jsonSettings;
        private Guid? id;
        public ResponseBuilderIsolated(IServiceProvider provider)
        {
            this.provider = provider;
        }



        public void WithJson(JsonSerializerSettings jsonSettings)
        {
            this.jsonSettings = jsonSettings;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete, list);
        }

        public ResponseBuilderIsolated WithContext<C>(IReadOnlyDictionary<string, object?> routeData) where C : IContextCreationServiceIsolated
        {
            if (contextCreateFunc == null)
            {
                contextCreateFunc = (ILogger log) => provider.GetService<C>().Create(routeData, log);
            }
            else
            {
                var existingCreate = contextCreateFunc;
                // execute multiple context creation services
                contextCreateFunc = async (ILogger log) =>
                {
                    await existingCreate.Invoke(log);
                    await provider.GetService<C>().Create(routeData, log);
                };
            }

            return this;
        }
        public ResponseBuilderIsolated WithId(Guid? id)
        {
            this.id = id;
            return this;
        }

        public ResponseBuilderWithIdEntityIsolated<T, T> ForEntity<T>() where T : class
        {
            var builder = new ResponseBuilderWithIdEntityIsolated<T, T>(provider, contextCreateFunc, jsonSettings, id);
            return builder;
        }
        public ResponseBuilderWithIdEntityIsolated<T, M> ForEntityWithMapping<T, M>() where T : class where M : class
        {
            var builder = new ResponseBuilderWithIdEntityIsolated<T, M>(provider, contextCreateFunc, jsonSettings, id);
            return builder;
        }

        public async Task<HttpResponseData> Handle<T, S>(HttpRequestData req, ILogger log) where T : class where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log);
        }
        public async Task<HttpResponseData> HandleAndMap<T, M, S>(HttpRequestData req, ILogger log) where T : class where M : class where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log);
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TServiceModel> Use<TServiceModel>() where TServiceModel : class
        {
            var service = provider.GetService<TServiceModel>();
            var mapper = GetMapperService();
            if (mapper == null) throw new Exception("Mapper is missing");

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new ResponseBuilderWithIdEntityServiceIsolated<TServiceModel>(service, contextCreateFunc, mapper, jsonSettings, id);
            return builder;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class where M : class
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

    public class ResponseBuilderWithIdEntityIsolated<T, M> where T : class where M : class
    {
        private readonly IServiceProvider provider;
        //private readonly bool usesMapping;
        private readonly Func<ILogger, Task> contextCreateFunc;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly Guid? id;

        public ResponseBuilderWithIdEntityIsolated(
            IServiceProvider provider,
            Func<ILogger, Task> contextCreateFunc,
            JsonSerializerSettings jsonSettings,
            Guid? id)
        {
            this.provider = provider;
            //this.usesMapping = !(typeof(T).Equals(typeof(M)));
            this.contextCreateFunc = contextCreateFunc;
            this.jsonSettings = jsonSettings;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, M, S>(service, contextCreateFunc, mapper, jsonSettings, id);
            return builder;
        }

        public ResponseBuilderWithIdEntityInterfaceServiceIsolated<T, M, S> UseInterface<S>() where S : class, ICRUDLServiceWithId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new ResponseBuilderWithIdEntityInterfaceServiceIsolated<T, M, S>(service, contextCreateFunc, mapper, jsonSettings, id);
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

    public class ResponseBuilderWithIdEntityInterfaceServiceIsolated<T, M, S> where S : class, ICRUDLServiceWithId<T> where M : class where T : class
    {
        private readonly ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, M, S> internalBuilder;
        private readonly Func<ILogger, Task> contextCreateFunc;
        public ResponseBuilderWithIdEntityInterfaceServiceIsolated(S service, Func<ILogger, Task> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings, Guid? id)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.contextCreateFunc = contextCreateFunc;

            internalBuilder = new ResponseBuilderWithIdEntityServiceWithModelsIsolated<T, M, S>(service, contextCreateFunc, mapper, jsonSettings, id);
            internalBuilder.OnPost(s => s.Create).OnGetWithId(s => s.Read).OnPut(s => s.Update).OnDeleteWithId(s => s.Delete).OnGet<IEnumerable<T>, IEnumerable<M>>(s => s.List);
        }
        public async Task<HttpResponseData> Handle(HttpRequestData req, ILogger log)
        {
            return await internalBuilder.Handle(req, log);
        }
    }

    public class ResponseBuilderWithIdEntityServiceIsolated<TService> : AbstractResponseHelpersIsolated where TService : class
    {
        private readonly TService service;
        private readonly Func<ILogger, Task> contextCreateFunc;
        private readonly Guid? id;

        private bool unwrap = false;
        private bool wrapout = false;
        public ResponseBuilderWithIdEntityServiceIsolated(TService service, Func<ILogger, Task> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings, Guid? id) : base(mapper, jsonSettings)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
            this.contextCreateFunc = contextCreateFunc;
            this.id = id;
        }



        #region Properties Config
        public ResponseBuilderWithIdEntityServiceIsolated<TService> UnwrapRequest()
        {
            unwrap = true;
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> NotUnwrapRequest()
        {
            unwrap = false;
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> WrapResponse()
        {
            wrapout = true;
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> NotWrapResponse()
        {
            wrapout = true;
            return this;
        }
        #endregion


        private Func<TService, HttpRequestData, Task<HttpResponseData>> getFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> getWithIdFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> postFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> postWithIdFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> putFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> putWithIdFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> deleteFunc;
        private Func<TService, HttpRequestData, Task<HttpResponseData>> deleteWithIdFunc;

        #region GET

        // no need for mapping outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGet<OutgoingModel>(Func<TService, Func<OutgoingModel>> predicate)
            where OutgoingModel : class
        {
            MakeGetFunc<OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGetWithId<Model>(Func<TService, Func<Guid, Model>> predicate)
            where Model : class
        {
            MakeGetWithIdFunc<Model, Model>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGet<OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingModel>>> predicate)
        where OutgoingModel : class
        {
            MakeGetFuncList<OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGetWithId<Model>(Func<TService, Func<Guid, IEnumerable<Model>>> predicate)
            where Model : class
        {
            MakeGetWithIdFuncList<Model, Model>(predicate);
            return this;
        }
        // with mapping outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGet<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
            where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakeGetFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGetWithId<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
            where OutgoingModel : class
            where OutgoingServiceModel : class
        {
            MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGet<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate)
         where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakeGetFuncList<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnGetWithId<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IEnumerable<OutgoingServiceModel>>> predicate)
            where OutgoingModel : class
            where OutgoingServiceModel : class
        {
            MakeGetWithIdFuncList<OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }




        #endregion

        #region POST
        // only model outgoing
        // no need for mapping ingoing and outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPost<Model>(Func<TService, Func<Model>> predicate) where Model : class
        {
            MakePostFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostWithId<Model>(Func<TService, Func<Guid, Model>> predicate) where Model : class
        {
            MakePostWithIdFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        // same model in and outgoing
        // no need for mapping ingoing and outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPost<Model>(Func<TService, Func<Model, Model>> predicate) where Model : class
        {
            MakePostFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostWithId<Model>(Func<TService, Func<Guid, Model, Model>> predicate) where Model : class
        {
            MakePostWithIdFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        // no need for mapping ingoing and outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPost<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            MakePostFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostWithId<IngoingModel, OutgoingModel>(Func<TService, Func<Guid, IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            MakePostWithIdFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        // no need for mapping ingoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPost<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePostFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostWithId<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePostWithIdFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        // all possible combination
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPost<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostWithId<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePostWithIdFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        #endregion

        #region POST FILE
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostFile<Model>(Func<TService, Func<string, string, long, Stream, Model>> predicate) where Model : class
        {
            MakePostFileFunc<Model, Model>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPostFileWithId<Model>(Func<TService, Func<Guid, string, string, long, Stream, Model>> predicate) where Model : class
        {
            MakePostFileWithIdFunc<Model, Model>(predicate);
            return this;
        }
        #endregion

        #region PUT

        // same model in and outgoing, no need for mapping ingoing and outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPut<Model>(Func<TService, Func<Model, Model>> predicate)
            where Model : class
        {
            MakePutFunc<Model, Model, Model, Model>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPutWithId<Model>(Func<TService, Func<Guid, Model, Model>> predicate)
            where Model : class
        {
            MakePutWithIdFunc<Model, Model, Model, Model>(predicate);
            return this;
        }

        // no need for mapping ingoing and outgoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPut<IngoingModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            MakePutFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPutWithId<IngoingModel, OutgoingModel>(Func<TService, Func<Guid, IngoingModel, OutgoingModel>> predicate) where IngoingModel : class where OutgoingModel : class
        {
            MakePutWithIdFunc<IngoingModel, IngoingModel, OutgoingModel, OutgoingModel>(predicate);
            return this;
        }

        // no need for mapping ingoing
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPut<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePutFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPutWithId<IngoingModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingModel, OutgoingServiceModel>> predicate) where IngoingModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePutWithIdFunc<IngoingModel, IngoingModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }

        // all possible combination
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPut<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePutFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnPutWithId<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingServiceModel, OutgoingServiceModel>> predicate) where IngoingModel : class where IngoingServiceModel : class where OutgoingModel : class where OutgoingServiceModel : class
        {
            MakePutWithIdFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(predicate);
            return this;
        }

        #endregion

        #region DELETE
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnDelete(Func<TService, Action> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceIsolated<TService> OnDeleteWithId(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFuncWithId(predicate);
            return this;
        }

        #endregion


        #region MakeFunctions

        protected void MakeGetFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            getFunc = async (TService service, HttpRequestData req) =>
            {
                var listResult = predicate.Invoke(service).Invoke();
                return Respond<OutgoingServiceModel, OutgoingModel>(req, listResult, wrapout);
            };
        }
        protected void MakeGetWithIdFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
          where OutgoingServiceModel : class
          where OutgoingModel : class
        {
            getWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                var idGuid = GetId(req);
                var resultRead = predicate.Invoke(service).Invoke(idGuid);
                if (resultRead != null)
                {
                    return Respond<OutgoingServiceModel, OutgoingModel>(req, resultRead, wrapout);
                }
                return RespondNotFound(req, new Exception(typeof(OutgoingModel).Name + " with id " + idGuid + " and service " + typeof(TService).Name + " not found"), wrapout);
            };
        }
        protected void MakeGetFuncList<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IEnumerable<OutgoingServiceModel>>> predicate)
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            getFunc = async (TService service, HttpRequestData req) =>
            {
                var listResult = predicate.Invoke(service).Invoke();
                return RespondList<OutgoingServiceModel, OutgoingModel>(req, listResult, wrapout);

            };
        }
        protected void MakeGetWithIdFuncList<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IEnumerable<OutgoingServiceModel>>> predicate)
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            getWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                var idGuid = GetId(req);
                var listResult = predicate.Invoke(service).Invoke(idGuid);
                return RespondList<OutgoingServiceModel, OutgoingModel>(req, listResult, wrapout);

            };
        }





        protected void MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<OutgoingServiceModel>> predicate)
            where IngoingModel : class
            where IngoingServiceModel : class
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel createData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke();

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }
        protected void MakePostWithIdFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, OutgoingServiceModel>> predicate)
           where IngoingModel : class
           where IngoingServiceModel : class
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            postWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel createData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);
                var idGuid = GetId(req);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke(idGuid);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }
        protected void MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
            where IngoingModel : class
            where IngoingServiceModel : class
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel createData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke(createData);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }
        protected void MakePostFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, Task<OutgoingServiceModel>>> predicate)
          where IngoingModel : class
          where IngoingServiceModel : class
          where OutgoingServiceModel : class
          where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel createData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = await predicate.Invoke(service).Invoke(createData);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }

        protected void MakePostWithIdFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingServiceModel, OutgoingServiceModel>> predicate)
         where IngoingModel : class
         where IngoingServiceModel : class
         where OutgoingServiceModel : class
         where OutgoingModel : class
        {
            postWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel createData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);
                var idGuid = GetId(req);

                if (createData == null) throw new ArgumentNullException();
                var resultCreated = predicate.Invoke(service).Invoke(idGuid, createData);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }
        protected void MakePostFileFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<string, string, long, Stream, OutgoingServiceModel>> predicate)
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            postFunc = async (TService service, HttpRequestData req) =>
            {
                var parser = await MultipartFormDataParser.ParseAsync(req.Body);
                var file = parser.Files.Where(f => f.Name == "file").FirstOrDefault();

                var resultCreated = predicate.Invoke(service).Invoke(file.FileName, file.ContentType, file.Data.Length, file.Data);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }
        protected void MakePostFileWithIdFunc<OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, string, string, long, Stream, OutgoingServiceModel>> predicate)
           where OutgoingServiceModel : class
           where OutgoingModel : class
        {
            postWithIdFunc = async (TService service, HttpRequestData req) =>
            {


                var parser = await MultipartFormDataParser.ParseAsync(req.Body);
                var file = parser.Files.Where(f => f.Name == "file").FirstOrDefault();
                var idGuid = GetId(req);

                var resultCreated = predicate.Invoke(service).Invoke(idGuid, file.FileName, file.ContentType, file.Data.Length, file.Data);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultCreated, wrapout);

            };
        }

        protected void MakePutFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<IngoingServiceModel, OutgoingServiceModel>> predicate)
         where IngoingModel : class
         where IngoingServiceModel : class
         where OutgoingServiceModel : class
         where OutgoingModel : class
        {
            putFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel updateData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);

                if (updateData == null) throw new ArgumentNullException();
                var resultUpdated = predicate.Invoke(service).Invoke(updateData);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultUpdated, wrapout);

            };
        }
        protected void MakePutWithIdFunc<IngoingModel, IngoingServiceModel, OutgoingServiceModel, OutgoingModel>(Func<TService, Func<Guid, IngoingServiceModel, OutgoingServiceModel>> predicate)
            where IngoingModel : class
            where IngoingServiceModel : class
            where OutgoingServiceModel : class
            where OutgoingModel : class
        {
            putWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                IngoingServiceModel updateData = await GetRequestBody<IngoingServiceModel, IngoingModel>(req, unwrap);
                var idGuid = GetId(req);

                if (updateData == null) throw new ArgumentNullException();
                var resultUpdated = predicate.Invoke(service).Invoke(idGuid, updateData);

                return Respond<OutgoingServiceModel, OutgoingModel>(req, resultUpdated, wrapout);

            };
        }

        protected void MakeDeleteFunc(Func<TService, Action> predicate)
        {
            deleteFunc = async (TService service, HttpRequestData req) =>
            {
                predicate.Invoke(service).Invoke();
                return RespondEmpty(req, wrapout);

            };
        }
        protected void MakeDeleteFuncWithId(Func<TService, Action<Guid>> predicate)
        {
            deleteWithIdFunc = async (TService service, HttpRequestData req) =>
            {
                var idGuid = GetId(req);

                predicate.Invoke(service).Invoke(idGuid);
                return RespondEmpty(req, wrapout);

            };
        }





        #endregion

        private Guid GetId(HttpRequestData req)
        {
            if (!id.HasValue) throw new ArgumentNullException("RouteValues do not contain id");
            return id.Value;
        }

        public async Task<HttpResponseData> Handle(HttpRequestData req, ILogger log)
        {
            log.LogInformation("ResponseBuilder handle function " + req.Method.ToUpper() + " " + typeof(TService).Name + "/" + typeof(TService).Name);

            if (contextCreateFunc != null) await contextCreateFunc.Invoke(log);

            try
            {
                if (id.HasValue)
                {
                    if (req.Method == "GET" && getWithIdFunc != null)
                    {
                        return await getWithIdFunc.Invoke(service, req);
                    }
                    if (req.Method == "POST" && postWithIdFunc != null)
                    {
                        return await postWithIdFunc.Invoke(service, req);
                    }
                    if (req.Method == "PUT" && putWithIdFunc != null)
                    {
                        return await putWithIdFunc.Invoke(service, req);
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
                    if (req.Method == "POST" && postFunc != null)
                    {
                        return await postFunc.Invoke(service, req);
                    }
                    if (req.Method == "PUT" && putFunc != null)
                    {
                        return await putFunc.Invoke(service, req);
                    }
                    if (req.Method == "DELETE" && deleteFunc != null)
                    {
                        return await deleteFunc.Invoke(service, req);
                    }
                }

                var message = req.Method + " method is not implemented in function";
                log.LogError(message);
                return RespondError(req, new NotImplementedException(message), wrapout, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return RespondError(req, ex, wrapout);
            }
        }

    }

    public class ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> : ResponseBuilderWithIdEntityServiceIsolated<TService> where TService : class where ServiceModel : class where ApiModel : class
    {


        public ResponseBuilderWithIdEntityServiceWithModelsIsolated(TService service, Func<ILogger, Task> contextCreateFunc, IEntityMapper mapper, JsonSerializerSettings jsonSettings, Guid? id) : base(service, contextCreateFunc, mapper, jsonSettings, id)
        {

        }

        public new ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> UnwrapRequest()
        {
            base.UnwrapRequest();
            return this;
        }
        public new ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> WrapResponse()
        {
            base.WrapResponse();
            return this;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> With(Func<TService, Func<ServiceModel, ServiceModel>> create, Func<TService, Func<Guid, ServiceModel>> read, Func<TService, Func<ServiceModel, ServiceModel>> update, Func<TService, Action<Guid>> delete, Func<TService, Func<IEnumerable<ServiceModel>>> list)
        {
            MakeGetFuncList<ServiceModel, ApiModel>(list);
            MakeGetWithIdFunc<ServiceModel, ApiModel>(read);
            MakePostFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(create);
            MakePutFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(update);
            MakeDeleteFuncWithId(delete);

            return this;
        }


        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnGet(Func<TService, Func<IEnumerable<ServiceModel>>> predicate)
        {
            MakeGetFuncList<ServiceModel, ApiModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnGet(Func<TService, Func<ServiceModel>> predicate)
        {
            MakeGetFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnGetWithId(Func<TService, Func<Guid, IEnumerable<ServiceModel>>> predicate)
        {
            MakeGetWithIdFuncList<ServiceModel, ApiModel>(predicate);
            return this;
        }

#nullable enable annotations
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnGetWithId(Func<TService, Func<Guid, ServiceModel?>> predicate)
        {
            MakeGetWithIdFunc<ServiceModel, ApiModel>(predicate);
            return this;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnPost(Func<TService, Func<ServiceModel, Task<ServiceModel>>> predicate)
        {
            MakePostFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnPost(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakePostFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnPostWithId(Func<TService, Func<Guid, ServiceModel, ServiceModel>> predicate)
        {
            MakePostWithIdFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }

        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnPut(Func<TService, Func<ServiceModel, ServiceModel>> predicate)
        {
            MakePutFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }
        public ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnPutWithId(Func<TService, Func<Guid, ServiceModel, ServiceModel>> predicate)
        {
            MakePutWithIdFunc<ApiModel, ServiceModel, ServiceModel, ApiModel>(predicate);
            return this;
        }

        public new ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnDelete(Func<TService, Action> predicate)
        {
            MakeDeleteFunc(predicate);
            return this;
        }
        public new ResponseBuilderWithIdEntityServiceWithModelsIsolated<ServiceModel, ApiModel, TService> OnDeleteWithId(Func<TService, Action<Guid>> predicate)
        {
            MakeDeleteFuncWithId(predicate);
            return this;
        }

    }
}
