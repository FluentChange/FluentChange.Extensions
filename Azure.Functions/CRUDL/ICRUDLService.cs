using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public interface ICRUDLService<T> where T : class
    {
        void Create(T todo);
        void Delete(Guid id);
        IEnumerable<T> List();
        T Read(Guid id);
        void Update(T todo);
    }
}
