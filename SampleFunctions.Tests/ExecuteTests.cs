using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.Testing;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.Common.Rest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace SampleFunctions.Tests
{
    public class ExecuteTests
    {
        [Test]
        public async Task Sample7ProductsCLviaRest()
        {
            var restClient = new InternalRestClient("http://localhost:7071");
            var rest = new CLFunctionsApiClient(restClient);

            var list = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(2, list.Data.Count());

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7ProductsCL.Create(newProduct);

            Assert.IsNotNull(created.Data);
            Assert.IsNotNull(created.Data.Id);
            Assert.AreEqual("New Product 1", created.Data.Name);

            var listNewAfterCreate = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(3, listNewAfterCreate.Data.Count());
        }

        [Test]
        public async Task ProductsDirect()
        {
            var dummyRestClient = new DirectInvokeFunctionClient<DemoCRUDLFunctions.Startup>();
            var rest = new CLFunctionsApiClient(dummyRestClient);

            var list = await rest.ExecuteReadProducts.ReadMutliple();
            Assert.AreEqual(2, list.Data.Count());         
        }

        [Test]
        public async Task ProductDirect()
        {
            var dummyRestClient = new DirectInvokeFunctionClient<DemoCRUDLFunctions.Startup>();
            var rest = new CLFunctionsApiClient(dummyRestClient);

            var response = await rest.ExecuteReadProduct.ReadSingle(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("Test Product 1", response.Data.Name);
        }


        [Test]
        public void DirectRestListRequest()
        {
            var rest = new RestSharp.RestClient("http://localhost:7071/api/");
            var request = new RestSharp.RestRequest("exe/products", RestSharp.Method.Get);

            var response = rest.ExecuteAsync<NewResponse<IEnumerable<ApiProduct>>>(request).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Data);
            Assert.AreEqual("Test Product 1", response.Data.Data.First().Name);
        }
    }
}
