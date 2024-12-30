using Microsoft.Extensions.DependencyInjection;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public interface IFunctionsStartUp
    {
        public void Configure(IServiceCollection services, string connectionString);
    }
}
