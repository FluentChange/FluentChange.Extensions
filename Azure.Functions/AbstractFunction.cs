using System;

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
        }

        public SingleBuilder Handler()
        {
            return new SingleBuilder(provider);
        }
    }
}
