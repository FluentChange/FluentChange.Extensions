﻿using Newtonsoft.Json;
using System;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class AbstractFunction    {
       
        protected readonly ResponseBuilderWithId ResponseBuilder;

        protected readonly IServiceProvider provider;
        public AbstractFunction(IServiceProvider provider)
        {
            this.provider = provider;
            this.ResponseBuilder = new ResponseBuilderWithId(provider);
        }

    }
}
