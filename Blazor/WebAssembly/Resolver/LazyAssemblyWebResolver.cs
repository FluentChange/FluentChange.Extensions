using FluentChange.Blazor.Interfaces;
using System;
using System.Collections.Generic;

namespace FluentChange.Blazor.WebAssembly
{
    public class LazyAssemblyWebResolver
    {
        private Dictionary<string, ILazyAssembly> lazyassemblies { get; set; }
        public LazyAssemblyWebResolver()
        {
            lazyassemblies = new Dictionary<string, ILazyAssembly>();
        }

        //private static string Plugin1 = "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin1.dll";
        //private static string Plugin2 = "https://myskipper.blob.core.windows.net/data/Sample.Blazor.LazyPlugin2.dll";

        public ILazyAssembly Resolve(string name)
        {
            //if (name == "Plugin1") return Plugin1;
            //if (name == "Plugin2") return Plugin2;
            //throw new ArgumentException();
            var assembly = lazyassemblies[name];
            return assembly;
        }

        public void Add(ILazyAssembly lazyassembly)
        {
            lazyassemblies.Add(lazyassembly.Name, lazyassembly);
        }

    }
}
