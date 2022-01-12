using DemoCRUDLFunctions.Models;
using FluentChange.Extensions.Common.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleFunctions.Tests
{
    public class CLFunctionsApiClient
    {
        private readonly IRestClient rest;

        public GenericCLApi<Todo> Sample1Todos { get; private set; }
        public GenericCLApi<Todo> Sample2Todos { get; private set; }
        public GenericCLApi<Event> Sample3Events { get; private set; }
        public GenericCLApi<Event> Sample4Events { get; private set; }
        public GenericCLApi<Event> Sample5Events { get; private set; }
        public WrappedGenericCLApi<Product> Sample6Products { get; private set; }
        public WrappedGenericCLApi<ApiProduct> Sample7Products { get; private set; }
        public CLFunctionsApiClient(IRestClient rest)
        {
            this.rest = rest;
            Sample1Todos = new GenericCLApi<Todo>(rest, "cl/sample1/todos", new Dictionary<string, object>());
            Sample2Todos = new GenericCLApi<Todo>(rest, "cl/sample2/todos", new Dictionary<string, object>());
            Sample3Events = new GenericCLApi<Event>(rest, "cl/sample3/events", new Dictionary<string, object>());
            Sample4Events = new GenericCLApi<Event>(rest, "cl/sample4/events", new Dictionary<string, object>());
            Sample5Events = new GenericCLApi<Event>(rest, "cl/sample5/events", new Dictionary<string, object>());
            Sample6Products = new WrappedGenericCLApi<Product>(rest, "cl/sample6/products", new Dictionary<string, object>());  
            Sample7Products = new WrappedGenericCLApi<ApiProduct>(rest, "cl/sample7/products", new Dictionary<string, object>());
     
        }


    }
}
