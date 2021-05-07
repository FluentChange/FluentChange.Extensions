using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentChange.Extensions.Azure.Functions.Helper;


namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class CRUDBuilder
    {
        private readonly IServiceProvider provider;
        public CRUDBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDBuilderEntity<T, T> ForEntity<T>() where T : new()
        {
            var builder = new CRUDBuilderEntity<T, T>(provider);
            return builder;
        }
        public CRUDBuilderEntity<T, M> ForEntityWithMapping<T, M>() where T : new() where M : new()
        {
            var builder = new CRUDBuilderEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : new() where S : class, ICRUDService<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : new() where M : new() where S : class, ICRUDService<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public CRUDBuilderEntityService<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete) where T : new() where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete);
        }
        public CRUDBuilderEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete) where T : new() where S : class where M : new()
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, read, update, delete);
        }
    }


    public class CRUDBuilderEntity<T, M> where T : new() where M : new()
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public CRUDBuilderEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public CRUDBuilderEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDBuilderEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public CRUDBuilderEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICRUDService<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CRUDBuilderEntityInterfaceService<T, M, S>(service, mapper);
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



    public class CRUDBuilderEntityInterfaceService<T, M, S> where S : class, ICRUDService<T> where M : new() where T : new()
    {
        private readonly CRUDBuilderEntityService<T, M, S> internalBuilder;
        public CRUDBuilderEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new CRUDBuilderEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class CRUDBuilderEntityService<T, M, S> : AbstractResponseHelpers<T, M> where S : class where T : new() where M : new()
    {
        private readonly S service;    

        public CRUDBuilderEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
        }

        private Func<S, Func<T, T>> createFunc;
        private Func<S, Func<T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action> deleteFunc;

        public CRUDBuilderEntityService<T, M, S> With(Func<S, Func<T, T>> create, Func<S, Func<T>> read, Func<S, Func<T, T>> update, Func<S, Action> delete)
        {
            createFunc = create;
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;

            return this;
        }            

        public CRUDBuilderEntityService<T, M, S> Create(Func<S, Func<T, T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
        public CRUDBuilderEntityService<T, M, S> Read(Func<S, Func<T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public CRUDBuilderEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public CRUDBuilderEntityService<T, M, S> Delete(Func<S, Action> predicate)
        {
            deleteFunc = predicate;
            return this;
        }
              

        public override async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
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
