using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoCRUDLFunctions
{
    public class Functions : CRUDLFunction
    {
        public Functions(IServiceProvider provider) : base(provider)
        {
        }

        [FunctionName("Sample1TodoCRUDL")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "sample1/todos" + CRUDLHelper.Id)] string id, HttpRequest req, ILogger log)
        {

            return await CRUDL
                .Handle<Todo, TodoService>(req, log, id);

        }

        [FunctionName("Sample2TodoCRUDL")]
        public async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "sample2/todos" + CRUDLHelper.Id)] string id, HttpRequest req, ILogger log)
        {

            return await CRUDL
                .ForEntity<Todo>()
                .UseInterface<TodoService>()
                .Handle(req, log, id);

        }

        [FunctionName("Sample3EventCRUDL")]
        public async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "sample3/events" + CRUDLHelper.Id)] string id, HttpRequest req, ILogger log)
        {

            return await CRUDL
                .With<Event,EventService>(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                .Handle(req, log, id);

        }

        [FunctionName("Sample4EventCRUDL")]
        public async Task<HttpResponseMessage> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "sample4/events" + CRUDLHelper.Id)] string id, HttpRequest req, ILogger log)
        {

            return await CRUDL
                 .ForEntity<Event>()
                 .Use<EventService>()
                 .With(s => s.New, s => s.Get, s => s.Edit, s => s.Remove, s => s.All)
                 .Handle(req, log, id);

        }

        [FunctionName("Sample5EventCRUDL")]
        public async Task<HttpResponseMessage> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "sample5/events" + CRUDLHelper.Id)] string id, HttpRequest req, ILogger log)
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
    }
}
