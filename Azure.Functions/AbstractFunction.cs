using System;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class AbstractFunction
    {
        protected readonly CRUDBuilder CRUD;
        protected readonly CRUDLBuilder CRUDL;
        public AbstractFunction(IServiceProvider provider)
        {       
            this.CRUDL = new CRUDLBuilder(provider);
            this.CRUD = new CRUDBuilder(provider);
        }
    
    }
}
