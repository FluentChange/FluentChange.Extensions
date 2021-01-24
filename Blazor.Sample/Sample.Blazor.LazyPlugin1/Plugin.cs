using FluentChange.Extensions.Blazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Test.BlazorPlugin1
{
    public class Plugin : IPlugin
    {
        public string Name => "Plugin 1";
        public void Init(IServiceCollection services)
        {
          
        }
        public string Execute(string input)
        {
            return $"The time is {DateTime.Now.ToString("hh:mm:ss")} (Request {input})";
        }
    }
}
