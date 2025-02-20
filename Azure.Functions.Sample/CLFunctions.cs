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
    public class CLFunctions(IServiceProvider provider, ILogger<CLFunctions> log) : AbstractFunction(provider)
    {
        [Function("Sample1TodoCL")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample1/todos" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .Handle<Todo, TodoService>(req, log);

        }

        [Function("Sample2TodoCL")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample2/todos" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log);

        }

        [Function("Sample3EventCL")]
        public async Task<IActionResult> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample3/events" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .With<Event, EventService>(s => s.New, null, null, null, s => s.All)
                .Handle(req, log);

        }

        [Function("Sample4EventCL")]
        public async Task<IActionResult> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample4/events" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, null, null, null, s => s.All)
                 .Handle(req, log);

        }

        [Function("Sample5EventCL")]
        public async Task<IActionResult> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample5/events" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .ForEntity<Event>()
                .Use<EventService>()
                .OnPost(s => s.New)
                .OnGet(s => s.All)
                .Handle(req, log);

        }

        [Function("Sample6ProductCL")]
        public async Task<IActionResult> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample6/products" + RouteHelper.Id)] HttpRequest req, string id)
        {

            return await ResponseBuilder
                .ForEntity<Product>()
                .Use<ProductService>()
                .UnwrapRequest()
                .With(s => s.Create, null, null, null, s => s.List)
                .WrapResponse()
                .Handle(req, log);

        }


        [Function("Sample7ProductMappingCL")]
        public async Task<IActionResult> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample7/products" + RouteHelper.Id)] HttpRequest req, string id)
        {
            try
            {
                return await ResponseBuilder
                  .ForEntityWithMapping<Product, ApiProduct>()
                  .Use<ProductService>()
                  .UnwrapRequest()
                  .With(s => s.Create, null, null, null, s => s.List)
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
