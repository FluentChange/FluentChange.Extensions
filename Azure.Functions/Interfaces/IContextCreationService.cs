using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Azure.Functions.Interfaces
{
    public interface IContextCreationService
    {
        Task Create(HttpRequest req);
    }

    public interface IContextCreationServiceNew
    {
        Task Create(HttpRequest req, ILogger logger);
    }

    public interface IContextCreationServiceIsolated
    {
        Task Create(IReadOnlyDictionary<string, object?> routeData, ILogger logger);
    }

}
