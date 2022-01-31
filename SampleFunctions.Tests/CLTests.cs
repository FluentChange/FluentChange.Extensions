using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.Testing;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.Common.Rest;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SampleFunctions.Tests
{
    public class CLTests
    {
        [Test]
        public async Task Sample7ProductsCLviaRest()
        {
            var restClient = new InternalRestClient("http://localhost:7071");
            var rest = new CLFunctionsApiClient(restClient);

            var list = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(2, list.Results.Count);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7ProductsCL.Create(newProduct);

            Assert.IsNotNull(created.Result);
            Assert.IsNotNull(created.Result.Id);
            Assert.AreEqual("New Product 1", created.Result.Name);

            var listNewAfterCreate = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(3, listNewAfterCreate.Results.Count);
        }

        [Test]
        public async Task Sample7ProductsCLdirect()
        {
            var dummyRestClient = new DirectInvokeFunctionClient<DemoCRUDLFunctions.Startup>();
            var rest = new CLFunctionsApiClient(dummyRestClient);

            var list = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(2, list.Results.Count);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7ProductsCL.Create(newProduct);

            Assert.IsNotNull(created.Result);
            Assert.IsNotNull(created.Result.Id);
            Assert.AreEqual("New Product 1", created.Result.Name);

            var listNewAfterCreate = await rest.Sample7ProductsCL.List();
            Assert.AreEqual(3, listNewAfterCreate.Results.Count);
        }




        [Test]
        public void DirectRestReadRequest()
        {
            var rest = new RestSharp.RestClient("http://localhost:7071/api/");
            var request = new RestSharp.RestRequest("cl/sample7/products", RestSharp.Method.GET);

            var response = rest.Execute<MultiResponse<ApiProduct>>(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Results);
            Assert.AreEqual("Test Product 1", response.Data.Results.First().Name);
        }
    }
}
