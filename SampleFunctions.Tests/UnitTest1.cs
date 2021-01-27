using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Common.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SampleFunctions.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rest = new FunctionsApiClient();

            //var list = rest.Sample7Products.List();
            //Assert.AreEqual(2, list.Results.Count);

            var read = rest.Sample7Products.Read(Guid.Parse("66bc54bf-9e0c-494d-84ad-cc239837f543"));
            Assert.IsNotNull(read);
            Assert.IsNotNull(read.Result);
            Assert.AreEqual("Test Product 1", read.Result.Name);
        }

        [TestMethod]
        public void TestMethod2()
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
