using DemoCRUDLFunctions.Services;
using FluentChange.AzureFunctions.CRUDL;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DemoCRUDLFunctions.Startup))]
namespace DemoCRUDLFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {   
            builder.Services.AddSingleton<TodoService>();
            builder.Services.AddSingleton<EventService>();
        }
    }
}
