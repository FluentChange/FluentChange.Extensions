using FluentChange.Blazor.Interfaces;
using Sample.Blazor.Shared.Services;
using System;


namespace Test.BlazorPlugin2.Services
{
    public class Service3: IScopedService
    {
        private AddService service1;
        public Service3(AddService service1)
        {
            Console.WriteLine("CTOR Service 3");
            this.service1 = service1;
        }
        public string AddSomething()
        {
            return "Service 3 - Add something " + service1.Add(3, 4);
        }
    }
}
