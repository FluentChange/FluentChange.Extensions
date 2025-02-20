using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DemoCRUDLFunctions
{
    public class CRUDLFunctions(IServiceProvider provider, ILogger<CRUDLFunctions> log) : AbstractFunction(provider)
    {
        [Function("Sample1TodoCRUDL")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample1/todos" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .Handle<Todo, TodoService>(req, log);

        }

        [Function("Sample2TodoCRUDL")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample2/todos" + RouteHelper.Id)] HttpRequest req, string id)
        {
            return await ResponseBuilder
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log);

        }

        [Function("Sample3EventCRUDL")]
        public async Task<IActionResult> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample3/events" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .With<Event, EventService>(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                .Handle(req, log);

        }

        [Function("Sample4EventCRUDL")]
        public async Task<IActionResult> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample4/events" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                 .Handle(req, log);

        }

        [Function("Sample5EventCRUDL")]
        public async Task<IActionResult> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample5/events" + RouteHelper.Id)] HttpRequest req, string id)
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

        [Function("Sample6ProductCRUDL")]
        public async Task<IActionResult> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample6/products" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .ForEntity<Product>()
                .Use<ProductService>()
                .UnwrapRequest()
                .With(s => s.Create, s => s.Read, s => s.Update, s => s.Delete, s => s.List)
                .WrapResponse()
                .Handle(req, log);

        }


        [Function("Sample7ProductMappingCRUDL")]
        public async Task<IActionResult> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "crudl/sample7/products" + RouteHelper.Id)] HttpRequest req, string id)
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
