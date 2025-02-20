using DemoCRUDLFunctions.Services;
using DemoCRUDLFunctions;
using FluentChange.Extensions.Azure.Functions.Helper;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleFunctions.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<TodoService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<EventService>();
builder.Services.AddSingleton<ContextCreationService>();
builder.Services.AddSingleton<IEntityMapper, MapperWrapper>();
builder.Services.AddAutoMapper(typeof(MapperProfile));

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
