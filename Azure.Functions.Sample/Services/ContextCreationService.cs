using FluentChange.Extensions.Azure.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleFunctions.Services
{
    public class ContextCreationService : IContextCreationService
    {
        public async Task Create(HttpRequest req)
        {
           // Do nothing for now
        }
    }
}
