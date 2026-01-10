using System;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class AbstractFunction    {
       
        protected readonly ResponseBuilderWithId ResponseBuilder;
        protected readonly ResponseBuilderIsolated ResponseBuilderIsolated;

        protected readonly IServiceProvider provider;
        public AbstractFunction(IServiceProvider provider)
        {
            this.provider = provider;
            this.ResponseBuilder = new ResponseBuilderWithId(provider);
            this.ResponseBuilderIsolated = new ResponseBuilderIsolated(provider);
        }

    }
}
