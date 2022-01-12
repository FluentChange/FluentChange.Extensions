using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Common.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleFunctions.Tests
{
    public class CRUDLFunctionsApiClient
    {
        private readonly IRestClient rest;

        public GenericCRUDLApi<Todo> Sample1Todos { get; private set; }
        public GenericCRUDLApi<Todo> Sample2Todos { get; private set; }
        public GenericCRUDLApi<Event> Sample3Events { get; private set; }
        public GenericCRUDLApi<Event> Sample4Events { get; private set; }
        public GenericCRUDLApi<Event> Sample5Events { get; private set; }
        public WrappedGenericCRUDLApi<Product> Sample6Products { get; private set; }
        public WrappedGenericCRUDLApi<ApiProduct> Sample7Products { get; private set; }
        public CRUDLFunctionsApiClient(IRestClient rest)
        {
            this.rest = rest;
            Sample1Todos = new GenericCRUDLApi<Todo>(rest, "crudl/sample1/todos", new Dictionary<string, string>());
            Sample2Todos = new GenericCRUDLApi<Todo>(rest, "crudl/sample2/todos", new Dictionary<string, string>());
            Sample3Events = new GenericCRUDLApi<Event>(rest, "crudl/sample3/events", new Dictionary<string, string>());
            Sample4Events = new GenericCRUDLApi<Event>(rest, "crudl/sample4/events", new Dictionary<string, string>());
            Sample5Events = new GenericCRUDLApi<Event>(rest, "crudl/sample5/events", new Dictionary<string, string>());
            Sample6Products = new WrappedGenericCRUDLApi<Product>(rest, "crudl/sample6/products", new Dictionary<string, string>());  
            Sample7Products = new WrappedGenericCRUDLApi<ApiProduct>(rest, "crudl/sample7/products", new Dictionary<string, string>());
     
        }


    }
}
