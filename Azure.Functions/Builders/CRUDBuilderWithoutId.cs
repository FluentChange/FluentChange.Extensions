using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Azure.Functions.Interfaces;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class CRUDBuilderWithoutId
    {
        private readonly IServiceProvider provider;
        public CRUDBuilderWithoutId(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDBuilderWithoutIdEntity<T, T> ForEntity<T>() where T : new()
        {
            var builder = new CRUDBuilderWithoutIdEntity<T, T>(provider);
            return builder;
        }
        public CRUDBuilderWithoutIdEntity<T, M> ForEntityWithMapping<T, M>() where T : new() where M : new()
        {
            var builder = new CRUDBuilderWithoutIdEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log) where T : new() where S : class, ICRUDServiceWithoutId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log) where T : new() where M : new() where S : class, ICRUDServiceWithoutId<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log);
        }

        public CRUDBuilderWithoutIdEntityService<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete) where T : new() where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete);
        }
        public CRUDBuilderWithoutIdEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete) where T : new() where S : class where M : new()
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, read, update, delete);
        }
    }


    public class CRUDBuilderWithoutIdEntity<T, M> where T : new() where M : new()
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public CRUDBuilderWithoutIdEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public CRUDBuilderWithoutIdEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDBuilderWithoutIdEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public CRUDBuilderWithoutIdEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDServiceWithoutId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDBuilderWithoutIdEntityInterfaceService<T, M, S>(service, mapper);
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



    public class CRUDBuilderWithoutIdEntityInterfaceService<T, M, S> where S : class, ICRUDServiceWithoutId<T> where M : new() where T : new()
    {
        private readonly CRUDBuilderWithoutIdEntityService<T, M, S> internalBuilder;
        public CRUDBuilderWithoutIdEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new CRUDBuilderWithoutIdEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            return await internalBuilder.Handle(req, log);
        }
    }

    public class CRUDBuilderWithoutIdEntityService<T, M, S> : AbstractResponseHelpersWithHandle<T, M> where S : class where T : new() where M : new()
    {
        private readonly S service;    

        public CRUDBuilderWithoutIdEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
        }

        private Func<S, Func<T, T>> createFunc;
        private Func<S, Func<T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action> deleteFunc;

        public CRUDBuilderWithoutIdEntityService<T, M, S> With(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete)
        {
            createFunc = create;
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;

            return this;
        }            

        public CRUDBuilderWithoutIdEntityService<T, M, S> Create(Func<S, Func<T, T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
        public CRUDBuilderWithoutIdEntityService<T, M, S> Read(Func<S, Func<T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public CRUDBuilderWithoutIdEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public CRUDBuilderWithoutIdEntityService<T, M, S> Delete(Func<S, Action> predicate)
        {
            deleteFunc = predicate;
            return this;
        }
              

        public override async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            log.LogInformation("CRUD Function " + req.Method.ToUpper() + " " + typeof(S).Name + "/" + typeof(T).Name);

            try
            {
                if (req.Method == "GET")
                {

                    if (readFunc == null) throw new NotImplementedException();                
                    var read = readFunc.Invoke(service).Invoke();

                    return Respond(read);

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
                    deleteFunc.Invoke(service).Invoke();
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
