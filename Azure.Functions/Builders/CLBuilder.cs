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
    public class CLBuilder
    {
        private readonly IServiceProvider provider;
        public CLBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CLBuilderEntity<T, T> ForEntity<T>() where T : class
        {
            var builder = new CLBuilderEntity<T, T>(provider);
            return builder;
        }
        public CLBuilderEntity<T, M> ForEntityWithMapping<T, M>() where T : class where M : class
        {
            var builder = new CLBuilderEntity<T, M>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log) where T : class where S : class, ICLService<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log);
        }

        public async Task<HttpResponseMessage> HandleAndMap<T, M, S>(HttpRequest req, ILogger log) where T : class where M : class where S : class, ICLService<T>
        {
            return await ForEntityWithMapping<T, M>().UseInterface<S>().Handle(req, log);
        }

        public CLBuilderEntityService<T, T, S> With<T, S>(Func<S, Func<T, T>> create, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class
        {
            return ForEntity<T>().Use<S>().With(create, list);
        }
        public CLBuilderEntityService<T, M, S> WithAndMap<T, M, S>(Func<S, Func<T, T>> create, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class where M : class
        {
            return ForEntityWithMapping<T, M>().Use<S>().With(create, list);
        }
    }


    public class CLBuilderEntity<T, M> where T : class where M : class
    {
        private readonly IServiceProvider provider;
        private readonly bool usesMapping;

        public CLBuilderEntity(IServiceProvider provider)
        {
            this.provider = provider;
            this.usesMapping = !(typeof(T).Equals(typeof(M)));
        }

        public CLBuilderEntityService<T, M, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();
            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CLBuilderEntityService<T, M, S>(service, mapper);
            return builder;
        }

        public CLBuilderEntityInterfaceService<T, M, S> UseInterface<S>() where S : class, ICLService<T>
        {
            var service = provider.GetService<S>();
            var mapper = GetMapperService();

            if (service == null) throw new NullReferenceException(nameof(service));
            if (mapper == null) throw new NullReferenceException(nameof(mapper));

            var builder = new CLBuilderEntityInterfaceService<T, M, S>(service, mapper);
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



    public class CLBuilderEntityInterfaceService<T, M, S> where S : class, ICLService<T> where M : class where T : class
    {
        private readonly CLBuilderEntityService<T, M, S> internalBuilder;
        public CLBuilderEntityInterfaceService(S service, IEntityMapper mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            internalBuilder = new CLBuilderEntityService<T, M, S>(service, mapper);
            internalBuilder.With(s => s.Create, s => s.List);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            return await internalBuilder.Handle(req, log);
        }
    }

    public class CLBuilderEntityService<T, M, S> : AbstractResponseHelpersWithHandle<T, M> where S : class where T : class where M : class
    {
        private readonly S service;

        public CLBuilderEntityService(S service, IEntityMapper mapper) : base(mapper)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            this.service = service;         
        }

        private Func<S, Func<T, T>> createFunc;
        private Func<S, Func<IEnumerable<T>>> listFunc;

        public CLBuilderEntityService<T, M, S> With(Func<S, Func<T, T>> create, Func<S, Func<IEnumerable<T>>> list)
        {
            createFunc = create;
            listFunc = list;
            return this;
        }

     
        public CLBuilderEntityService<T, M, S> Create(Func<S, Func<T, T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
       
        public CLBuilderEntityService<T, M, S> List(Func<S, Func<IEnumerable<T>>> predicate)
        {
            listFunc = predicate;
            return this;
        }
                
        public override async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log)
        {
            log.LogInformation("CL Function " + req.Method.ToUpper() + " " + typeof(S).Name + "/" + typeof(T).Name);

            try
            {
                if (req.Method == "GET")
                {
                    if (listFunc == null) throw new NotImplementedException();
                    var list = listFunc.Invoke(service).Invoke();
                    return Respond(list);                   
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
                    throw new NotImplementedException();                   
                }
                if (req.Method == "DELETE")
                {
                    throw new NotImplementedException();
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
