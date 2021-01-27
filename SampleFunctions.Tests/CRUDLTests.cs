using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Common.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SampleFunctions.Tests
{
    [TestClass]
    public class CRUDLTests
    {
        [TestMethod]
        public void Sample7Products()
        {
            var rest = new FunctionsApiClient();

            var list = rest.Sample7Products.List();
            Assert.AreEqual(2, list.Results.Count);

            var read = rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Result);
            Assert.AreEqual("Test Product 1", read.Result.Name);

            var newProduct = new ApiProduct()
            {
                Name = "New Product 1",
                Text = "Description New Product 1",
                Price = 555.5
            };

            var created = rest.Sample7Products.Create(newProduct);

            Assert.IsNotNull(created.Result);
            Assert.IsNotNull(created.Result.Id);
            Assert.AreEqual("New Product 1", created.Result.Name);

            var listNewAfterCreate = rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterCreate.Results.Count);

            created.Result.Name = "Updated Name";
            var updated = rest.Sample7Products.Update(created.Result);
            Assert.AreEqual("Updated Name", updated.Result.Name);

            var listNewAfterUpdate = rest.Sample7Products.List();
            Assert.AreEqual(3, listNewAfterUpdate.Results.Count);

            rest.Sample7Products.Delete(created.Result.Id);

            var listNewAfterDelete = rest.Sample7Products.List();
            Assert.AreEqual(2, listNewAfterDelete.Results.Count);
        }

        [TestMethod]
        public void DirectRequest()
        {
            var rest = new RestSharp.RestClient("http://localhost:7071/api/");
            var request = new RestSharp.RestRequest("sample7/products/66bc54bf-9e0c-494d-84ad-cc239837f543", RestSharp.Method.GET);

            var response = rest.Execute<SingleResponse<ApiProduct>>(request);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Result);
            Assert.AreEqual("Test Product 1", response.Data.Result.Name);
        }
    }


}
