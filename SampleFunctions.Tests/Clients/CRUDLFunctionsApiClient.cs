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

        public GenericCRUDLWithIdApi<Todo> Sample1Todos { get; private set; }
        public GenericCRUDLWithIdApi<Todo> Sample2Todos { get; private set; }
        public GenericCRUDLWithIdApi<Event> Sample3Events { get; private set; }
        public GenericCRUDLWithIdApi<Event> Sample4Events { get; private set; }
        public GenericCRUDLWithIdApi<Event> Sample5Events { get; private set; }
        public WrappedGenericCRUDLWithIdApi<Product> Sample6Products { get; private set; }
        public WrappedGenericCRUDLWithIdApi<ApiProduct> Sample7Products { get; private set; }
        public CRUDLFunctionsApiClient(IRestClient rest)
        {
            this.rest = rest;
            Sample1Todos = new GenericCRUDLWithIdApi<Todo>(rest, "crudl/sample1/todos", new Dictionary<string, object>());
            Sample2Todos = new GenericCRUDLWithIdApi<Todo>(rest, "crudl/sample2/todos", new Dictionary<string, object>());
            Sample3Events = new GenericCRUDLWithIdApi<Event>(rest, "crudl/sample3/events", new Dictionary<string, object>());
            Sample4Events = new GenericCRUDLWithIdApi<Event>(rest, "crudl/sample4/events", new Dictionary<string, object>());
            Sample5Events = new GenericCRUDLWithIdApi<Event>(rest, "crudl/sample5/events", new Dictionary<string, object>());
            Sample6Products = new WrappedGenericCRUDLWithIdApi<Product>(rest, "crudl/sample6/products", new Dictionary<string, object>());  
            Sample7Products = new WrappedGenericCRUDLWithIdApi<ApiProduct>(rest, "crudl/sample7/products", new Dictionary<string, object>());     
        }
    }
}
