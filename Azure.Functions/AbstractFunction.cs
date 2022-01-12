using System;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class AbstractFunction
    {
        protected readonly RUDBuilder RUD;
        protected readonly CLBuilder CL;
        protected readonly CRUDBuilder CRUD;
        protected readonly CRUDLBuilder CRUDL;
        public AbstractFunction(IServiceProvider provider)
        {       
            this.CRUDL = new CRUDLBuilder(provider);
            this.CRUD = new CRUDBuilder(provider);
            this.CL = new CLBuilder(provider);
            this.RUD = new RUDBuilder(provider);
        }
    
    }
}
