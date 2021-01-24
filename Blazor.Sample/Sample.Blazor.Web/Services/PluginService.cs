using FluentChange.Extensions.Blazor.Interfaces;
using FluentChange.Extensions.Blazor.WebAssembly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sample.Blazor.Web.Services
{

    public class HostedPlugin
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
    }

    public class PluginService : ISingletonService
    {
        //private Assembly SystemRuntime = Assembly.Load(new AssemblyName("System.Runtime"));
        public Dictionary<string, List<string>> PluginResponses { get; private set; } = new Dictionary<string, List<string>>();
        public List<HostedPlugin> Plugins { get; set; } = new List<HostedPlugin>();

        public void LoadPlugins()
        {
            var assembliesPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            foreach (var pluginfolder in Directory.EnumerateDirectories(assembliesPath))
            {
                var pluginName = Path.GetFileName(pluginfolder);
                if (Plugins.FirstOrDefault(f => f.Name == pluginName) == null)
                {
                    Plugins.Add(new HostedPlugin
                    {
                        Name = pluginName,
                        FilePath = Path.Combine(pluginfolder, $"{pluginName}.dll"),
                    });
                }
            }
        }

        // put entire UnloadableAssemblyLoadContext in a method to avoid caller
        // holding on to the reference
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExecuteAssembly(HostedPlugin plugin, string input)
        {
            var context = new CollectibleAssemblyContext();
            var assemblyPath = Path.Combine(plugin.FilePath);
            using (var fs = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read))
            {
                var assembly = context.LoadFromStream(fs);

                var type = assembly.GetType("PluginSystem.Plugin");
                var executeMethod = type.GetMethod("Execute");

                var instance = Activator.CreateInstance(type);

                var dic = PluginResponses.GetOrCreate(plugin.Name);

                dic.Add(executeMethod.Invoke(instance, new object[] { input }).ToString());
            }

            context.Unload();
        }


        public void RunPlugin(HostedPlugin plugin, string input)
        {
            ExecuteAssembly(plugin, input);

            RunGarbageCollection();
        }


        private static void RunGarbageCollection()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (System.Exception)
            {
                //sometimes GC.Collet/WaitForPendingFinalizers crashes, just ignore for this blog post
            }
        }      
    }
    public static class Extensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }
    }
}
