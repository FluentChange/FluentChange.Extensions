using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Common.Rest;
using System.Linq;
using SystemNet = System.Net;


namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class CRUDLBuilder
    {
        private readonly IServiceProvider provider;
        public CRUDLBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDLBuilderEntity<T, T> ForEntity<T>() where T : new()
        {
            var builder = new CRUDLBuilderEntity<T, T>(provider);
            return builder;
        }
        public CRUDLBuilderEntity<T, M> ForEntityWithMapping<T, M>() where T : new() where M : new()
        {
            var builder = new CRUDLBuilderEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : new() where S : class, ICRUDLService<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : new() where M : new() where S : class, ICRUDLService<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public CRUDLBuilderEntityService<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : new() where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete, list);
        }
        public CRUDLBuilderEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : new() where S : class where M : new()
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, read, update, delete, list);
        }
    }


    public class CRUDLBuilderEntity<T, M> where T : new() where M : new()
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public CRUDLBuilderEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public CRUDLBuilderEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDLBuilderEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public CRUDLBuilderEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDLService<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDLBuilderEntityInterfaceService<T, M, S>(service, mapper);
            return builder;
        }
        private IEntityMapper GetMapperService()
        {
            IEntityMapper mapper = null;
            if (usesMapping)
            {
                mapper = provider.GetService<IEntityMapper>();
                if (mapper == null) throw new Exception("Mapper is missing");
            }

            return mapper;
        }
    }



    public class CRUDLBuilderEntityInterfaceService<T, M, S> where S : class, ICRUDLService<T> where M : new() where T : new()
    {
        private readonly CRUDLBuilderEntityService<T, M, S> internalBuilder;
        public CRUDLBuilderEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new CRUDLBuilderEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class CRUDLBuilderEntityService<T, M, S> where S : class where T : new() where M : new()
    {
        private readonly S service;
        private readonly IEntityMapper mapper;
        private bool wrapRequestAndResponse;
        private readonly bool usesMapping;
        private JsonSerializerSettings jsonSettings;

        public CRUDLBuilderEntityService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
            this.mapper = mapper;
            this.wrapRequestAndResponse = false;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        private Func<S, Func<T, T>> createFunc;
        private Func<S, Func<Guid, T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action<Guid>> deleteFunc;
        private Func<S, Func<IEnumerable<T>>> listFunc;

        public CRUDLBuilderEntityService<T, M, S> With(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list)
        {
            createFunc = create;
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;
            listFunc = list;
            return this;
        }

        public CRUDLBuilderEntityService<T, M, S> WithJson(JsonSerializerSettings settings)
        {
            this.jsonSettings = settings;
            return this;
        }

        public CRUDLBuilderEntityService<T, M, S> Create(Func<S, Func<T, T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, M, S> Read(Func<S, Func<Guid, T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, M, S> Delete(Func<S, Action<Guid>> predicate)
        {
            deleteFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, M, S> List(Func<S, Func<IEnumerable<T>>> predicate)
        {
            listFunc = predicate;
            return this;
        }

        public CRUDLBuilderEntityService<T, M, S> WrapRequestAndResponse()
        {
            wrapRequestAndResponse = true;
            return this;
        }

        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("CRUDL Function " + req.Method.ToUpper() + " " + typeof(S).Name + "/" + typeof(T).Name);

            try
            {
                if (req.Method == "GET")
                {

                    if (!String.IsNullOrEmpty(id))
                    {
                        if (readFunc == null) throw new NotImplementedException();
                        var idGuid = Guid.Parse(id);
                        var read = readFunc.Invoke(service).Invoke(idGuid);

                        return Respond(read);
                    }
                    else
                    {
                        if (listFunc == null) throw new NotImplementedException();
                        var list = listFunc.Invoke(service).Invoke();
                        return Respond(list);
                    }
                }
                if (req.Method == "POST")
                {
                    if (createFunc == null) throw new NotImplementedException();
                    T create = await GetRequestBody(req);

                    if (create == null) throw new ArgumentNullException();
                    var resultCreated = createFunc.Invoke(service).Invoke(create);

                    return Respond(resultCreated);
                }
                if (req.Method == "PUT")
                {
                    if (updateFunc == null) throw new NotImplementedException();
                    T update = await GetRequestBody(req);

                    if (update == null) throw new ArgumentNullException();
                    var resultUpdated = updateFunc.Invoke(service).Invoke(update);

                    return Respond(resultUpdated);
                }
                if (req.Method == "DELETE")
                {
                    if (deleteFunc == null) throw new NotImplementedException();
                    if (String.IsNullOrEmpty(id)) throw new ArgumentNullException();
                    var idGuid = Guid.Parse(id);

                    deleteFunc.Invoke(service).Invoke(idGuid);
                    return Respond();
                }
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return RespondError(ex);
            }

        }

        private async Task<T> GetRequestBody(HttpRequest req)
        {
            if (req.Body == null) throw new ArgumentNullException();
            if (req.Body.Length == 0) throw new ArgumentNullException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (wrapRequestAndResponse)
            {
                if (usesMapping)
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<M>>(requestBody);
                    var entityMapped = mapper.MapTo<T>(entityWrapped.Data);
                    return entityMapped;
                }
                else
                {
                    var entityWrapped = JsonConvert.DeserializeObject<SingleRequest<T>>(requestBody);
                    return entityWrapped.Data;
                }
            }
            else
            {
                if (usesMapping)
                {
                    var entity = JsonConvert.DeserializeObject<M>(requestBody);
                    var entityMapped = mapper.MapTo<T>(entity);
                    return entityMapped;
                }
                else
                {
                    var entity = JsonConvert.DeserializeObject<T>(requestBody);
                    return entity;
                }
            }
        }
        private HttpResponseMessage RespondError(Exception ex)
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
        private HttpResponseMessage Respond()
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

        private HttpResponseMessage Respond(T result)
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

        private HttpResponseMessage Respond(IEnumerable<T> results)
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
    }
}
