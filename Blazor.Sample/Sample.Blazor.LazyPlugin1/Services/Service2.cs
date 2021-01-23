using FluentChange.Blazor.Interfaces;
using Sample.Blazor.Shared.Services;
using System;

namespace Test.BlazorPlugin1
{
    public class Service2: IScopedService
    {
        private AddService service1;
        public Service2(AddService service1)
        {
            Console.WriteLine("CTOR Service 2");
            this.service1 = service1;
        }
        public string AddSomething()
        {
            return "Service 2 - Add something " + service1.Add(1, 2);
        }
    }
}
