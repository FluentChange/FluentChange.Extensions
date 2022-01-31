using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class AbstractFunction
    {
        protected readonly CLBuilder CL;
        protected readonly RUDBuilderWithId RUDwithId;
        protected readonly RUDBuilderWithoutId RUDwithoutId;
        protected readonly CRUDBuilderWithoutId CRUDwithoutId;
        protected readonly CRUDBuilderWithId CRUDwithId;
        protected readonly CRUDLBuilderWithId CRUDLwithId;
        protected readonly SingleBuilderWithId Handler;

        protected readonly IServiceProvider provider;
        public AbstractFunction(IServiceProvider provider)
        {
            this.provider = provider;
            this.CL = new CLBuilder(provider);
            this.RUDwithId = new RUDBuilderWithId(provider);
            this.RUDwithoutId = new RUDBuilderWithoutId(provider);
            this.CRUDwithoutId = new CRUDBuilderWithoutId(provider);
            this.CRUDwithId = new CRUDBuilderWithId(provider);
            this.CRUDLwithId = new CRUDLBuilderWithId(provider);
            this.Handler = new SingleBuilderWithId(provider);
        }
    }
}
