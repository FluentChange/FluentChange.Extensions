using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.AzureFunctions.CRUDL
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
