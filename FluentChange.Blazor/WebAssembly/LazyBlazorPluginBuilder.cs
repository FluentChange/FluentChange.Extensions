namespace FluentChange.Blazor.WebAssembly
{
    public class LazyBlazorPluginBuilder
    {
        private LazyAssemblyWebResolver resolver;
        public LazyBlazorPluginBuilder(LazyAssemblyWebResolver resolver)
        {
            this.resolver = resolver;
        }

        public LazyBlazorPluginBuilder AddWebPlugin(string name, string url)
        {
            var assembly = new WebLazyAssembly();
            assembly.Name = name;
            assembly.Url = url;
            resolver.Add(assembly);

            return this;
        }
        public LazyBlazorPluginBuilder AddNugetPlugin(string name, string package)
        {
            var assembly = new NugetLazyAssembly();
            assembly.Name = name;
            assembly.Package = package;
            resolver.Add(assembly);

            return this;
        }
    }
}
