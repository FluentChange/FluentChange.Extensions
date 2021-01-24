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

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class CRUDLBuilder
    {
        private readonly IServiceProvider provider;
        public CRUDLBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDLBuilderEntity<T> ForEntity<T>() where T : class
        {
            var builder = new CRUDLBuilderEntity<T>(provider);
            return builder;
        }

        public async Task<HttpResponseMessage> Handle<T, S>(HttpRequest req, ILogger log, string id) where T : class where S : class, ICRUDLService<T>
        {
            return await ForEntity<T>().UseInterface<S>().Handle(req, log, id);
        }

        public CRUDLBuilderEntityService<T, S> With<T, S>(Func<S, Action<T>> create, Func<S, Func<Guid, T>> read, Func<S, Action<T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list) where T : class where S : class
        {
            return ForEntity<T>().Use<S>().With(create, read, update, delete, list);
        }
    }


    public class CRUDLBuilderEntity<T> where T : class
    {
        private readonly IServiceProvider provider;

        public CRUDLBuilderEntity(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public CRUDLBuilderEntityService<T, S> Use<S>() where S : class
        {
            var service = provider.GetService<S>();
            var builder = new CRUDLBuilderEntityService<T, S>(service);
            return builder;
        }
    
        public CRUDLBuilderEntityInterfaceService<T, S> UseInterface<S>() where S : class, ICRUDLService<T>
        {
            var service = provider.GetService<S>();
            var builder = new CRUDLBuilderEntityInterfaceService<T, S>(service);
            return builder;
        }
    }

      

    public class CRUDLBuilderEntityInterfaceService<T, S> where S : class, ICRUDLService<T> where T : class
    {
        private readonly CRUDLBuilderEntityService<T, S> internalBuilder;
        public CRUDLBuilderEntityInterfaceService(S service)
        {

            internalBuilder = new CRUDLBuilderEntityService<T, S>(service);
            internalBuilder.With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List);
        }
        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {
            return await internalBuilder.Handle(req, log, id);
        }
    }

    public class CRUDLBuilderEntityService<T, S> where S : class where T : class
    {
        private readonly S service;
        private bool useResponseWrapper;

        public CRUDLBuilderEntityService(S service)
        {
            this.service = service;
            this.useResponseWrapper = false;
        }

        private Func<S, Action<T>> createFunc;
        private Func<S, Func<Guid, T>> readFunc;
        private Func<S, Action<T>> updateFunc;
        private Func<S, Action<Guid>> deleteFunc;
        private Func<S, Func<IEnumerable<T>>> listFunc;

        public CRUDLBuilderEntityService<T, S> With(Func<S, Action<T>> create, Func<S, Func<Guid, T>> read, Func<S, Action<T>> update, Func<S, Action<Guid>> delete, Func<S, Func<IEnumerable<T>>> list)
        {
            createFunc = create;
            readFunc = read;
            updateFunc = update;
            deleteFunc = delete;
            listFunc = list;
            return this;
        }

        public CRUDLBuilderEntityService<T, S> Create(Func<S, Action<T>> predicate)
        {
            createFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, S> Read(Func<S, Func<Guid, T>> predicate)
        {
            readFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, S> Update(Func<S, Action<T>> predicate)
        {
            updateFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, S> Delete(Func<S, Action<Guid>> predicate)
        {
            deleteFunc = predicate;
            return this;
        }
        public CRUDLBuilderEntityService<T, S> List(Func<S, Func<IEnumerable<T>>> predicate)
        {
            listFunc = predicate;
            return this;
        }

        public CRUDLBuilderEntityService<T, S>  WrapResponse ()
        {
            useResponseWrapper = true;
            return this;
        }

        public async Task<HttpResponseMessage> Handle(HttpRequest req, ILogger log, string id)
        {

            log.LogInformation("CRUDL Function " + req.Method.ToUpper() + " " + typeof(S).Name + "/" + typeof(T).Name);

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
                if (req.Body == null) throw new ArgumentNullException();
                if (req.Body.Length == 0) throw new ArgumentNullException();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var todo = JsonConvert.DeserializeObject<T>(requestBody);

                if (todo == null) throw new ArgumentNullException();
                createFunc.Invoke(service).Invoke(todo);
                return Respond();

            }
            if (req.Method == "PUT")
            {
                if (updateFunc == null) throw new NotImplementedException();
                if (req.Body == null) throw new ArgumentNullException();
                if (req.Body.Length == 0) throw new ArgumentNullException();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var todo = JsonConvert.DeserializeObject<T>(requestBody);

                if (todo == null) throw new ArgumentNullException();

                updateFunc.Invoke(service).Invoke(todo);
                return Respond(); 
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



        private HttpResponseMessage Respond()
        {
            if (useResponseWrapper)
            {
                var response = new Response();                
                return ResponseHelper.CreateJsonResponse(response);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(null);
            }
        }

        private HttpResponseMessage Respond(T result)
        {
            if (useResponseWrapper)
            {
                var response = new SingleResponse<T>();
                response.Result = result;
                return ResponseHelper.CreateJsonResponse(response);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(result);
            }
        }

        private HttpResponseMessage Respond(IEnumerable<T> results)
        {
            if (useResponseWrapper)
            {
                var response = new MultiResponse<T>();
                response.Results = results.ToList();
                return ResponseHelper.CreateJsonResponse(response);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(results);
            }
        }
    }
}



