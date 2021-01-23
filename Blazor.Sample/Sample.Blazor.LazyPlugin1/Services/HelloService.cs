using FluentChange.Blazor.Interfaces;

namespace Test.BlazorPlugin1
{
    public class HelloService : IScopedService
    {
        public string GetHello(string name)
        {
            return "Hello " + name;
        }
    }
}
