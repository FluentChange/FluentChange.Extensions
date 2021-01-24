using FluentChange.Extensions.Blazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using Syncfusion.Licensing;
using System;

namespace Test.BlazorPlugin2
{
    public class Plugin : IPlugin
    {
        public string Name => "Plugin 2";

        public void Init(IServiceCollection services)
        {
            Console.WriteLine("Init " + Name);
            //SyncfusionLicenseProvider.RegisterLicense("....");
            services.AddSyncfusionBlazor();



        }

        public string Execute(string input)
        {
            return $"Hello World! (Request {input})";
        }
    }
}
