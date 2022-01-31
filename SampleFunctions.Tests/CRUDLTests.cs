using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.Testing;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.Common.Rest;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SampleFunctions.Tests
{

    public class CRUDLTests
    {
        [Test]
        public async Task Sample7ProductsCRUDLviaRest()
        {
            var restClient = new InternalRestClient("http://localhost:7071");
            var rest = new CRUDLFunctionsApiClient(restClient);

            var list = await rest.Sample7Products.List();
            Assert.AreEqual(2, list.Results.Count);

            var read = await rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Result);
            Assert.AreEqual("Test Product 1", read.Result.Name);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7Products.Create(newProduct);

            Assert.IsNotNull(created.Result);
            Assert.IsNotNull(created.Result.Id);
            Assert.AreEqual("New Product 1", created.Result.Name);

            var listNewAfterCreate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterCreate.Results.Count);

            created.Result.Name = "Updated Name";
            var updated = await rest.Sample7Products.Update(created.Result);
            Assert.AreEqual("Updated Name", updated.Result.Name);

            var listNewAfterUpdate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterUpdate.Results.Count);

            var deleted = await rest.Sample7Products.Delete(created.Result.Id);
            Assert.IsNotNull(deleted);
            if (deleted.Errors != null) Assert.AreEqual(0, deleted.Errors.Count);

            var listNewAfterDelete = await rest.Sample7Products.List();
            Assert.AreEqual(2, listNewAfterDelete.Results.Count);
        }

        [Test]
        public async Task Sample7ProductsCRUDLdirect()
        {
            var dummyRestClient = new DirectInvokeFunctionClient<DemoCRUDLFunctions.Startup>();
            var rest = new CRUDLFunctionsApiClient(dummyRestClient);

            var list = await rest.Sample7Products.List();
            Assert.AreEqual(2, list.Results.Count);

            var read = await rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Result);
            Assert.AreEqual("Test Product 1", read.Result.Name);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7Products.Create(newProduct);

            Assert.IsNotNull(created.Result);
            Assert.IsNotNull(created.Result.Id);
            Assert.AreEqual("New Product 1", created.Result.Name);

            var listNewAfterCreate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterCreate.Results.Count);

            created.Result.Name = "Updated Name";
            var updated = await rest.Sample7Products.Update(created.Result);
            Assert.AreEqual("Updated Name", updated.Result.Name);

            var listNewAfterUpdate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterUpdate.Results.Count);

            var deleted = await rest.Sample7Products.Delete(created.Result.Id);
            Assert.IsNotNull(deleted);
            if (deleted.Errors != null) Assert.AreEqual(0, deleted.Errors.Count);

            var listNewAfterDelete = await rest.Sample7Products.List();
            Assert.AreEqual(2, listNewAfterDelete.Results.Count);
        }

        [Test]
        public void DirectRestRequest()
        {
            var rest = new RestSharp.RestClient("http://localhost:7071/api/");
            var request = new RestSharp.RestRequest("crudl/sample7/products/66bc54bf-9e0c-494d-84ad-cc239837f543", RestSharp.Method.GET);

            var response = rest.Execute<SingleResponse<ApiProduct>>(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Result);
            Assert.AreEqual("Test Product 1", response.Data.Result.Name);
        }

      
    }


}
