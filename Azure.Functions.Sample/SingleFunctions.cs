using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoCRUDLFunctions
{
    public class SingleFunctions : AbstractFunction
    {       
        public SingleFunctions(IServiceProvider provider) : base(provider)
        {         
        }

        [FunctionName("SingleFunctions")]
        public async Task<HttpResponseMessage> Run7([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "single/products")] HttpRequest req, ILogger log)
        {
            return await ResponseBuilder
                .Use<ProductService>()
                .OnGet<Product, ApiProduct>(service => service.List)
                .WrapResponse()
                .Handle(req, log);
        }
    }
}
