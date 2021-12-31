using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FluentChange.Extensions.Azure.Functions.Testing
{
    public class DummyFunctionsHostBuilder : IFunctionsHostBuilder
    {
        public IServiceCollection Services { get; private set; }
        public DummyFunctionsHostBuilder(IServiceCollection services)
        {
            this.Services = services;
        }
    }
}
