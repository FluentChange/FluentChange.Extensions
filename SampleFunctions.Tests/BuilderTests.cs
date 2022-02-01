using DemoCRUDLFunctions;
using DemoCRUDLFunctions.Models;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Azure.Functions.Testing;
using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using SampleFunctions.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SampleFunctions.Tests
{
    public class BuilderTests
    {
        [Test]
        public async Task SingleHandlerTests()
        {
            var logger = new ConsoleListLogger();
            var services = new ServiceCollection();
            services.AddSingleton<ProductService>();
            services.AddSingleton<IEntityMapper, MapperWrapper>();
            services.AddSingleton<ILogger, ListLogger>();
            services.AddAutoMapper(typeof(MapperProfile));

            var provider = services.BuildServiceProvider();

            var Single = new SingleBuilder(provider);

            var settings = new Newtonsoft.Json.JsonSerializerSettings();


            var updatedProd = new Product()
            {
                Id = Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"),
                Title = "Updated",
                Description = "Updated",
                Price = 99
            };
            var updatedApiProd = new ApiProduct()
            {
                Id = Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"),
                Name = "Updated",
                Text = "Updated",
                Price = 99
            };
            var wrapped = new SingleRequest<ApiProduct>()
            {
                Data = updatedApiProd
            };
            var apiProdReq = await CreateDummyRequestWithBody(updatedApiProd);
            var wrappedApiProdReq = await CreateDummyRequestWithBody(wrapped);
            var wrappedApiProdReq2 = await CreateDummyRequestWithBody(wrapped);
            var wrappedApiProdReq3 = await CreateDummyRequestWithBody(wrapped);
            var wrappedApiProdReq4 = await CreateDummyRequestWithBody(wrapped);


            var handlerInputGuidWithConfig = await Single
               .Config(settings, logger)
               .WithInput(() => Guid.NewGuid())
               .Use<ProductService>()
               .Execute<Product>(service => service.Read)
               .MapTo<ApiProduct>()
               .WrapOutput()
               .Respond();

            var handlerInputGuid = await Single
                .Config(logger)
                .WithInput(() => Guid.NewGuid())
                .Use<ProductService>()
                .Execute<Product>(service => service.Read)
                .MapTo<ApiProduct>()
                .WrapOutput()
                .Respond();

            var handlerInputProduct = await Single       
                .Config(logger)               
                .WithInput(updatedProd)
                .Use<ProductService>()
                .Execute<Product>(service => service.Update)
                .MapTo<ApiProduct>()
                .Respond();

            var handlerInputApiProduct = await Single
                .Config(logger)
                .WithInput(updatedApiProd)
                .MapInputTo<Product>()
                .Use<ProductService>()
                .Execute<Product>(service => service.Update)
                .MapTo<ApiProduct>()
                .Respond();

            var handlerInputRequest = await Single
                .Config(logger)
                .WithBody<ApiProduct>(apiProdReq)
                .MapInputTo<Product>()
                .Use<ProductService>()
                .Execute<Product>(service => service.Update)
                .MapTo<ApiProduct>()
                .Respond();

            var handler7a = await Single
                .Config(logger)
                .With<ContextCreationService>(wrappedApiProdReq)
                .WithUnwrappingAndMapping<ApiProduct, Product>(wrappedApiProdReq)
                .Use<ProductService>()
                .Execute<Product>(service => service.Update)
                .MapTo<ApiProduct>()
                .WrapOutput()
                .Respond();

            var handler7aAsync = await Single
                .Config(logger)
                .WithUnwrappingAndMapping<ApiProduct, Product>(wrappedApiProdReq2)
                .Use<ProductService>()
                .Execute<Product>(service => service.UpdateAsync)
                .MapTo<ApiProduct>()
                .WrapOutput()
                .Respond();

            var handler7b = await Single
                .Config(logger)
                .WithUnwrappingAndMapping<ApiProduct, Product>(wrappedApiProdReq3)
                .Use<ProductService>()
                .Execute((service, input) => service.Update(input))
                .MapTo<ApiProduct>()
                .WrapOutput()
                .Respond();

            var handlerAsync = await Single
                .Config(logger)
                .WithUnwrappingAndMapping<ApiProduct, Product>(wrappedApiProdReq4)
                .Use<ProductService>()
                .Execute((service, input) => service.UpdateAsync(input))
                .MapTo<ApiProduct>()
                .WrapOutput()
                .Respond();

            var handlerList = await Single
                .Config(logger)
                .Use<ProductService>()
                .Execute<IEnumerable<Product>>(service => service.List)
                .MapTo<IEnumerable<ApiProduct>>()
                .WrapOutput()
                .Respond();

            Assert.AreEqual(HttpStatusCode.OK, handlerInputGuidWithConfig.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerInputGuid.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerInputProduct.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerInputApiProduct.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerInputRequest.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handler7a.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handler7aAsync.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handler7b.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerAsync.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, handlerList.StatusCode);
        }


        private static async Task<HttpRequest> CreateDummyRequestWithBody(object bodyData)
        {
            var dummyHttpContext = new DefaultHttpContext();
            var req = dummyHttpContext.Request;
            var content = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8, "application/json");
            var stream = await content.ReadAsStreamAsync();
            req.Body = stream;
            return req;
        }
    }
}
