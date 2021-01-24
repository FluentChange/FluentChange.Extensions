using FluentChange.Extensions.Blazor.Interfaces;
using FluentChange.Extensions.Blazor.Services;
using FluentChange.Extensions.Blazor.WebAssembly;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sample.Blazor.Web.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.Blazor.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.UseLazyBlazorPlugins()
                .AddWebPlugin("Plugin1", "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin1.dll")
                .AddWebPlugin("Plugin2", "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin2.dll");

            // Microsoft DI
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // not sure why, but need to AddSingleton here manually, can't use reflection
            builder.Services.AddSingleton<LazyLoaderService>();
            builder.Services.AddSingleton<TestLocalStorage>();

            await builder.Build().RunAsync();
        }
    }
}
