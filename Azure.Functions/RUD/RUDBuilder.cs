using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentChange.Extensions.Azure.Functions.Helper;


namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class RUDBuilder
    {
        private readonly IServiceProvider provider;
        public RUDBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public RUDBuilderEntity<T, T> ForEntity<T>() where T : new()
        {
            var builder = new RUDBuilderEntity<T, T>(provider);
            return builder;
        }
        public RUDBuilderEntity<T, M> ForEntityWithMapping<T, M>() where T : new() where M : new()
        {
            var builder = new RUDBuilderEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : new() where S : class, IRUDService<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : new() where M : new() where S : class, IRUDService<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public RUDBuilderEntityService<T, T, S> With<T, S>(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete) where T : new() where S : class
        {
            return ForEntity<T>().Use<S>().With(read, update, delete);
        }
        public RUDBuilderEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete) where T : new() where S : class where M : new()
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(read, update, delete);
        }
    }


    public class RUDBuilderEntity<T, M> where T : new() where M : new()
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public RUDBuilderEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public RUDBuilderEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new RUDBuilderEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public RUDBuilderEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, IRUDService<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new RUDBuilderEntityInterfaceService<T, M, S>(service, mapper);
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



    public class RUDBuilderEntityInterfaceService<T, M, S> where S : class, IRUDService<T> where M : new() where T : new()
    {
        private readonly RUDBuilderEntityService<T, M, S> internalBuilder;
        public RUDBuilderEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new RUDBuilderEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Read, s => s.Update, s => s.Delete);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class RUDBuilderEntityService<T, M, S> : AbstractResponseHelpersWithHandleId<T, M> where S : class where T : new() where M : new()
    {
        private readonly S service;

        public RUDBuilderEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
        }

        private Func<S, Func<Guid, T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action<Guid>> deleteFunc;

        public RUDBuilderEntityService<T, M, S> With(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete)
        {
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;
            return this;
        }

        public RUDBuilderEntityService<T, M, S> Read(Func<S, Func<Guid, T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public RUDBuilderEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public RUDBuilderEntityService<T, M, S> Delete(Func<S, Action<Guid>> predicate)
        {
            deleteFunc = predicate;
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
                        throw new NotImplementedException();
                    }
                }
                if (req.Method == "POST")
                {
                    throw new NotImplementedException();
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
