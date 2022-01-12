using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Azure.Functions.Interfaces;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class CRUDLBuilderWithId
    {
        private readonly IServiceProvider provider;
        public CRUDLBuilderWithId(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDLBuilderWithIdEntity<T, T> ForEntity<T>() where T : new()
        {
            var builder = new CRUDLBuilderWithIdEntity<T, T>(provider);
            return builder;
        }
        public CRUDLBuilderWithIdEntity<T, M> ForEntityWithMapping<T, M>() where T : new() where M : new()
        {
            var builder = new CRUDLBuilderWithIdEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : new() where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : new() where M : new() where S : class, ICRUDLServiceWithId<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public CRUDLBuilderWithIdEntityService<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : new() where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete, list);
        }
        public CRUDLBuilderWithIdEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : new() where S : class where M : new()
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, read, update, delete, list);
        }
    }


    public class CRUDLBuilderWithIdEntity<T, M> where T : new() where M : new()
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public CRUDLBuilderWithIdEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public CRUDLBuilderWithIdEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDLBuilderWithIdEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public CRUDLBuilderWithIdEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDLServiceWithId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDLBuilderWithIdEntityInterfaceService<T, M, S>(service, mapper);
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



    public class CRUDLBuilderWithIdEntityInterfaceService<T, M, S> where S : class, ICRUDLServiceWithId<T> where M : new() where T : new()
    {
        private readonly CRUDLBuilderWithIdEntityService<T, M, S> internalBuilder;
        public CRUDLBuilderWithIdEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new CRUDLBuilderWithIdEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class CRUDLBuilderWithIdEntityService<T, M, S> : AbstractResponseHelpersWithHandleId<T, M> where S : class where T : new() where M : new()
    {
        private readonly S service;

        public CRUDLBuilderWithIdEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;         
        }

        private Func<S, Func<T, T>> createFunc;
        private Func<S, Func<Guid, T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action<Guid>> deleteFunc;
        private Func<S, Func<IEnumerable<T>>> listFunc;

        public CRUDLBuilderWithIdEntityService<T, M, S> With(Func<S, Func<T, T>> create, Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list)
        {
            createFunc = create;
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;
            listFunc = list;
            return this;
        }

     
        public CRUDLBuilderWithIdEntityService<T, M, S> Create(Func<S, Func<T, T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
        public CRUDLBuilderWithIdEntityService<T, M, S> Read(Func<S, Func<Guid, T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public CRUDLBuilderWithIdEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public CRUDLBuilderWithIdEntityService<T, M, S> Delete(Func<S, Action<Guid>> predicate)
        {
            deleteFunc = predicate;
            return this;
        }
        public CRUDLBuilderWithIdEntityService<T, M, S> List(Func<S, Func<IEnumerable<T>>> predicate)
        {
            listFunc = predicate;
            return this;
        }
                
        public override async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
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

      
    }
}
