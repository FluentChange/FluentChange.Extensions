using System.Reflection;
using System.Runtime.Loader;

namespace FluentChange.Blazor.WebAssembly
{
    public class CollectibleAssemblyContext : AssemblyLoadContext
    {
        public CollectibleAssemblyContext() : base(isCollectible: true)  // true allows us to unload the assemblies
        {
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
