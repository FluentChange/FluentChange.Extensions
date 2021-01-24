using Microsoft.Extensions.DependencyInjection;

namespace FluentChange.Extensions.Blazor.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }

        void Init(IServiceCollection services);

        string Execute(string input);
    }
}
