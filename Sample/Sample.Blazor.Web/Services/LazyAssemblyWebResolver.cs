using FluentChange.Blazor.Interfaces;
using System;

namespace Sample.Blazor.Web.Services
{
    public class LazyAssemblyWebResolver: ILazyAssemblyLocationResolver
    {
        private static string Plugin1 = "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin1.dll";
        private static string Plugin2 = "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin2.dll";

        public string Resolve(string name)
        {
            if (name == "Plugin1") return Plugin1;
            if (name == "Plugin2") return Plugin2;
            throw new ArgumentException();
        }
    }
}
