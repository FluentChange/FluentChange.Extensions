using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Azure.Functions.Testing;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.Common.Rest;
using NUnit.Framework;
using RestSharp;
using System;
using System.Linq;
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
            Assert.AreEqual(2, list.Data.Count());

            var read = await rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Data);
            Assert.AreEqual("Test Product 1", read.Data.Name);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7Products.Create(newProduct);

            Assert.IsNotNull(created.Data);
            Assert.IsNotNull(created.Data.Id);
            Assert.AreEqual("New Product 1", created.Data.Name);

            var listNewAfterCreate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterCreate.Data.Count());

            created.Data.Name = "Updated Name";
            var updated = await rest.Sample7Products.Update(created.Data);
            Assert.AreEqual("Updated Name", updated.Data.Name);

            var listNewAfterUpdate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterUpdate.Data.Count());

            var deleted = await rest.Sample7Products.Delete(created.Data.Id);
            Assert.IsNotNull(deleted);
            if (deleted.Errors != null) Assert.AreEqual(0, deleted.Errors.Count);

            var listNewAfterDelete = await rest.Sample7Products.List();
            Assert.AreEqual(2, listNewAfterDelete.Data.Count());
        }

        [Test]
        public async Task Sample7ProductsCRUDLdirect()
        {
            var dummyRestClient = new DirectInvokeFunctionClient<DemoCRUDLFunctions.Startup>();
            var rest = new CRUDLFunctionsApiClient(dummyRestClient);

            var list = await rest.Sample7Products.List();
            Assert.AreEqual(2, list.Data.Count());

            var read = await rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Data);
            Assert.AreEqual("Test Product 1", read.Data.Name);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = await rest.Sample7Products.Create(newProduct);

            Assert.IsNotNull(created.Data);
            Assert.IsNotNull(created.Data.Id);
            Assert.AreEqual("New Product 1", created.Data.Name);

            var listNewAfterCreate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterCreate.Data.Count());

            created.Data.Name = "Updated Name";
            var updated = await rest.Sample7Products.Update(created.Data);
            Assert.AreEqual("Updated Name", updated.Data.Name);

            var listNewAfterUpdate = await rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterUpdate.Data.Count());

            var deleted = await rest.Sample7Products.Delete(created.Data.Id);
            Assert.IsNotNull(deleted);
            if (deleted.Errors != null) Assert.AreEqual(0, deleted.Errors.Count);

            var listNewAfterDelete = await rest.Sample7Products.List();
            Assert.AreEqual(2, listNewAfterDelete.Data.Count());
        }

        [Test]
        public void DirectRestRequest()
        {
            var rest = new RestSharp.RestClient("http://localhost:7071/api/");
            var request = new RestSharp.RestRequest("crudl/sample7/products/66bc54bf-9e0c-494d-84ad-cc239837f543", RestSharp.Method.Get);

            var response = rest.ExecuteAsync<NewResponse<ApiProduct>>(request).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Data);
            Assert.AreEqual("Test Product 1", response.Data.Data.Name);
        }


    }


}
