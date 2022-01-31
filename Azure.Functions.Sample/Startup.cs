using AutoMapper;
using DemoCRUDLFunctions.Services;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using FluentChange.Extensions.Azure.Functions.Helper;
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
            builder.Services.AddSingleton<ProductService>();
            builder.Services.AddSingleton<EventService>();
            builder.Services.AddSingleton<IEntityMapper, MapperWrapper>();
            builder.Services.AddAutoMapper(typeof(MapperProfile));
        }
    }
}
