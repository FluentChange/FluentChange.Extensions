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
        private readonly LightInject.ServiceContainer container;
        public readonly IEnumerable<Assembly> alreadyBaseLoaded;
        public readonly ILazyAssemblyLocationResolver assemblyLocationResolver;

        public LazyLoaderService(ServiceContainer container, ILazyAssemblyLocationResolver assemblyLocationResolver)
        {
            this.container = container;
            this.assemblyLocationResolver = assemblyLocationResolver;

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
    }
}
