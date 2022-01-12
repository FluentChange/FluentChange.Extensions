using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoCRUDLFunctions
{
    public class CRUDLFunctions : AbstractFunction
    {
        public CRUDLFunctions(IServiceProvider provider) : base(provider)
        {
        }

        [FunctionName("Sample1TodoCRUDL")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample1/todos" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CRUDL
                .Handle<Todo, TodoService>(req, log, id);

        }

        [FunctionName("Sample2TodoCRUDL")]
        public async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample2/todos" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {
            return await CRUDL
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log, id);

        }

        [FunctionName("Sample3EventCRUDL")]
        public async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample3/events" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CRUDL
                .With<Event, EventService>(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                .Handle(req, log, id);

        }

        [FunctionName("Sample4EventCRUDL")]
        public async Task<HttpResponseMessage> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample4/events" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CRUDL
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                 .Handle(req, log, id);

        }

        [FunctionName("Sample5EventCRUDL")]
        public async Task<HttpResponseMessage> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample5/events" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CRUDL
                .ForEntity<Event>()
                .Use<EventService>()
                .Create(s => s.New)
                .Read(s => s.Get)
                .Update(s => s.Edit)
                .Delete(s => s.Remove)
                .List(s => s.All)
                .Handle(req, log, id);
        }

        [FunctionName("Sample6ProductCRUDL")]
        public async Task<HttpResponseMessage> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample6/products" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CRUDL
                .ForEntity<Product>()
                .Use<ProductService>()
                .With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List)
                .WrapRequestAndResponse()
                .Handle(req, log, id);

        }


        [FunctionName("Sample7ProductMappingCRUDL")]
        public async Task<HttpResponseMessage> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample7/products" + CRUDLHelper.Id)] HttpRequest req, string id, ILogger log)
        {
            try
            {
                return await CRUDL
              .ForEntityWithMapping<Product, ApiProduct>()
              .Use<ProductService>()
              .With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List)
              .WrapRequestAndResponse()
              .Handle(req, log, id);
            }
            catch (Exception ex)
            {
                return ResponseHelper.CreateJsonResponse(ex.ToString(), System.Net.HttpStatusCode.InternalServerError);
            }


        }
    }
}
