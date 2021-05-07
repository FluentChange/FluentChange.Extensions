using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public interface ICRUDService<T> where T : new()
    {
        T Create(T todo);
        void Delete();
        T Read();
        T Update(T todo);
    }
       
}
