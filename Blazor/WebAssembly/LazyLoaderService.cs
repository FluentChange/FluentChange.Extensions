using Blazored.LocalStorage;
using FluentChange.Blazor.Interfaces;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentChange.Blazor.WebAssembly
{
    public class LazyLoaderService: ISingletonService
    {

        private readonly Dictionary<string, Assembly> alreadyLazyLoaded;
        private readonly ServiceContainer container;
        public readonly IEnumerable<Assembly> alreadyBaseLoaded;
        public readonly LazyAssemblyWebResolver assemblyLocationResolver;
        private readonly ISyncLocalStorageService localStorage;
        public LazyLoaderService(ServiceContainer container, LazyAssemblyWebResolver assemblyLocationResolver, ISyncLocalStorageService localStorage)
        {
            this.container = container;
            this.assemblyLocationResolver = assemblyLocationResolver;
            this.localStorage = localStorage;

            alreadyLazyLoaded = new Dictionary<string, Assembly>();
            //loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name + ".dll").ToHashSet();
            alreadyBaseLoaded = getLoadedAssemblies();
        }


        public IEnumerable<Assembly> getLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft")).OrderBy(a => a.FullName).ToList();
        }

        public async Task<Assembly> LoadAssembliesFromWeb(string name)
        {
            var assemblyToLoad = assemblyLocationResolver.Resolve(name);

            if (assemblyToLoad is WebLazyAssembly) return await LoadAssemblyFromWebUrl(assemblyToLoad as WebLazyAssembly);
            if (assemblyToLoad is NugetLazyAssembly) return await LoadAssemblyFromNuget(assemblyToLoad as NugetLazyAssembly);
            throw new NotImplementedException();
        }

        private async Task<Assembly> LoadAssemblyFromWebUrl(WebLazyAssembly lazywebassembly)
        {
            var assemblyToLoad = lazywebassembly.Url;
            Console.WriteLine("TRY LOAD " + assemblyToLoad);
            Assembly newLoadedAssembly;

            foreach (var loaded in alreadyLazyLoaded)
            {
                Console.WriteLine("  ALREADY LOADED " + loaded);
            }


            if (!alreadyLazyLoaded.ContainsKey(assemblyToLoad))
            {

                var client = new HttpClient();
                var content = await client.GetStreamAsync(assemblyToLoad);
                Console.WriteLine("  DOWNLOADED " + assemblyToLoad);
                // The runtime loads assemblies into an isolated context by default. As a result,
                // assemblies that are loaded via Assembly.Load aren't available in the app's context
                // AKA the default context. To work around this, we explicitly load the assemblies
                // into the default app context.

                lock (alreadyLazyLoaded)
                {
                    // BECAUSE of async operation during download another Component i.e. could try to load
                    if (!alreadyLazyLoaded.ContainsKey(assemblyToLoad))
                    {


                        var context = new CollectibleAssemblyContext();
                        // load
                        var loadedAssembly = context.LoadFromStream(content);
                        //var loadedAssembly = AssemblyLoadContext.Default.LoadFromStream(content);
                        Console.WriteLine("  LOADED " + assemblyToLoad);

                        // init
                        var iPluginType = typeof(IPlugin);
                        var pluginClass = loadedAssembly.GetTypes().SingleOrDefault(t => iPluginType.IsAssignableFrom(t));

                        if (pluginClass != null)
                        {
                            var plugin = (IPlugin)Activator.CreateInstance(pluginClass);

                            var x = new ServiceCollection();
                            plugin.Init(x);
                            container.AddFrom(x);
                        }


                        // depency injection add services
                        container.AddAllInterfaceServices(loadedAssembly);

                        // avoid reloading                     
                        alreadyLazyLoaded.Add(assemblyToLoad, loadedAssembly);
                        newLoadedAssembly = loadedAssembly;
                    }
                    else
                    {
                        newLoadedAssembly = alreadyLazyLoaded[assemblyToLoad];
                    }
                }
            }
            else
            {
                newLoadedAssembly = alreadyLazyLoaded[assemblyToLoad];
            }

            return newLoadedAssembly;
        }
        private async Task<Assembly> LoadAssemblyFromNuget(NugetLazyAssembly lazywebassembly)
        {
            throw new NotImplementedException();
        }
    }
}
