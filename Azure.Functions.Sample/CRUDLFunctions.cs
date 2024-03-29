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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample1/todos" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await ResponseBuilder
                .Handle<Todo, TodoService>(req, log);

        }

        [FunctionName("Sample2TodoCRUDL")]
        public async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample2/todos" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {
            return await ResponseBuilder
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log);

        }

        [FunctionName("Sample3EventCRUDL")]
        public async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample3/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await ResponseBuilder
                .With<Event, EventService>(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                .Handle(req, log);

        }

        [FunctionName("Sample4EventCRUDL")]
        public async Task<HttpResponseMessage> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample4/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await ResponseBuilder
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                 .Handle(req, log);

        }

        [FunctionName("Sample5EventCRUDL")]
        public async Task<HttpResponseMessage> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample5/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await ResponseBuilder
                .ForEntity<Event>()
                .Use<EventService>()
                .OnPost(s => s.New)
                .OnGetWithId(s => s.Get)
                .OnPut(s => s.Edit)
                .OnDeleteWithId(s => s.Remove)
                .OnGet(s => s.All)
                .Handle(req, log);
        }

        [FunctionName("Sample6ProductCRUDL")]
        public async Task<HttpResponseMessage> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample6/products" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await ResponseBuilder
                .ForEntity<Product>()
                .Use<ProductService>()
                .UnwrapRequest()
                .With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List)
                .WrapResponse()
                .Handle(req, log);

        }


        [FunctionName("Sample7ProductMappingCRUDL")]
        public async Task<HttpResponseMessage> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample7/products" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {
            try
            {
                return await ResponseBuilder
              .ForEntityWithMapping<Product, ApiProduct>()
              .Use<ProductService>()
              .UnwrapRequest()
              .With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List)
              .WrapResponse()
              .Handle(req, log);
            }
            catch (Exception ex)
            {
                return ResponseHelper.CreateJsonResponse(ex.ToString(), System.Net.HttpStatusCode.InternalServerError);
            }


        }
    }
}
