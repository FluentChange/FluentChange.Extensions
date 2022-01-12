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
    public class CLFunctions : AbstractFunction
    {
        public CLFunctions(IServiceProvider provider) : base(provider)
        {
        }

        [FunctionName("Sample1TodoCL")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample1/todos" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                .Handle<Todo, TodoService>(req, log);

        }

        [FunctionName("Sample2TodoCL")]
        public async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample2/todos" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log);

        }

        [FunctionName("Sample3EventCL")]
        public async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample3/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                .With<Event, EventService>(s => s.New, s => s.All)
                .Handle(req, log);

        }

        [FunctionName("Sample4EventCL")]
        public async Task<HttpResponseMessage> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample4/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, s => s.All)
                 .Handle(req, log);

        }

        [FunctionName("Sample5EventCL")]
        public async Task<HttpResponseMessage> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample5/events" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                .ForEntity<Event>()
                .Use<EventService>()
                .Create(s => s.New)
                .List(s => s.All)
                .Handle(req, log);

        }

        [FunctionName("Sample6ProductCL")]
        public async Task<HttpResponseMessage> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample6/products" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {

            return await CL
                .ForEntity<Product>()
                .Use<ProductService>()
                .With(s => s.Create, s => s.List)
                .WrapRequestAndResponse()
                .Handle(req, log);

        }


        [FunctionName("Sample7ProductMappingCL")]
        public async Task<HttpResponseMessage> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cl/sample7/products" + RouteHelper.Id)] HttpRequest req, string id, ILogger log)
        {
            try
            {
                return await CL
                  .ForEntityWithMapping<Product, ApiProduct>()
                  .Use<ProductService>()
                  .With(s => s.Create, s => s.List)
                  .WrapRequestAndResponse()
                  .Handle(req, log);
            }
            catch (Exception ex)
            {
                return ResponseHelper.CreateJsonResponse(ex.ToString(), System.Net.HttpStatusCode.InternalServerError);
            }


        }
    }
}
