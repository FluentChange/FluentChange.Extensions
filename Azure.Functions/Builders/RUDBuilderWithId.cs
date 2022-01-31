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
    public class RUDBuilderWithId
    {
        private readonly IServiceProvider provider;
        public RUDBuilderWithId(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public RUDBuilderWithIdEntity<T, T> ForEntity<T>() where T : class
        {
            var builder = new RUDBuilderWithIdEntity<T, T>(provider);
            return builder;
        }
        public RUDBuilderWithIdEntity<T, M> ForEntityWithMapping<T, M>() where T : class where M : class
        {
            var builder = new RUDBuilderWithIdEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : class where S : class, IRUDServiceWithId<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log, string id) where T : class where M : class where S : class, IRUDServiceWithId<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log, id);
        }

        public RUDBuilderWithIdEntityService<T, T, S> With<T, S>(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete) where T : class where S : class
        {
            return ForEntity<T>().Use<S>().With(read, update, delete);
        }
        public RUDBuilderWithIdEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete) where T : class where S : class where M : class
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(read, update, delete);
        }
    }


    public class RUDBuilderWithIdEntity<T, M> where T : class where M : class
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public RUDBuilderWithIdEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public RUDBuilderWithIdEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new RUDBuilderWithIdEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public RUDBuilderWithIdEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, IRUDServiceWithId<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new RUDBuilderWithIdEntityInterfaceService<T, M, S>(service, mapper);
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



    public class RUDBuilderWithIdEntityInterfaceService<T, M, S> where S : class, IRUDServiceWithId<T> where M : class where T : class
    {
        private readonly RUDBuilderWithIdEntityService<T, M, S> internalBuilder;
        public RUDBuilderWithIdEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new RUDBuilderWithIdEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Read, s => s.Update, s => s.Delete);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class RUDBuilderWithIdEntityService<T, M, S> : AbstractResponseHelpersWithHandleId<T, M> where S : class where T : class where M : class
    {
        private readonly S service;

        public RUDBuilderWithIdEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;
        }

        private Func<S, Func<Guid, T>> readFunc;
        private Func<S, Func<T, T>> updateFunc;
        private Func<S, Action<Guid>> deleteFunc;

        public RUDBuilderWithIdEntityService<T, M, S> With(Func<S, Func<Guid, T>> read, Func<S, Func<T, T>> update, Func<S, Action<Guid>> delete)
        {
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;
            return this;
        }

        public RUDBuilderWithIdEntityService<T, M, S> Read(Func<S, Func<Guid, T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public RUDBuilderWithIdEntityService<T, M, S> Update(Func<S, Func<T, T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public RUDBuilderWithIdEntityService<T, M, S> Delete(Func<S, Action<Guid>> predicate)
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
