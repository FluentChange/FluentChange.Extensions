using FluentChange.Extensions.Common.Database.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.Common.Database
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonDatabase(this IServiceCollection services)
        {
            services.AddScoped<UserContextService>();
        }
    }
}
