using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DemoCRUDLFunctions
{
    public class SingleFunctions(IServiceProvider provider, ILogger<SingleFunctions> log) : AbstractFunction(provider)
    {
        [Function("SingleFunctions")]
        public async Task<IActionResult> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "single/products")] HttpRequest req)
        {
            return await ResponseBuilder
                .Use<ProductService>()
                .OnGet<Product, ApiProduct>(service => service.List)
                .WrapResponse()
                .Handle(req, log);
        }
    }
}
