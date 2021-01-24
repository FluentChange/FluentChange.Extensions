
using FluentChange.Extensions.Blazor.Interfaces;

namespace Sample.Blazor.Web.Services
{


    public class TestXService: IScopedService
    {
        public TestXService()
        {
   
        }

        public string AddSubservices()
        {
            return "oki";
        }

    }
}
