using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

}
