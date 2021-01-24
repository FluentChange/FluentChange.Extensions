using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public abstract class CRUDLFunction
    {  
        protected readonly CRUDLBuilder CRUDL;
        public CRUDLFunction(IServiceProvider provider)
        {       
            this.CRUDL = new CRUDLBuilder(provider);
        }
    }
}
