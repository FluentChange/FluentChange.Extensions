using FluentChange.Blazor.Interfaces;

namespace Sample.Blazor.Web.Services
{
    public class TestYService : IScopedService
    {
        public TestYService()
        {

        }

        public string HelloY()
        {
           
            return "why !?";
        }  
    }
}
