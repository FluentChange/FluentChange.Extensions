using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public interface ICRUDServiceOld<T> where T : new()
    {
        T Create(T todo);
        void Delete();
        T Read();
        T Update(T todo);
    }

    public interface ICService<T> where T : new()
    {
        T Create(T element);
    }
    public interface ILService<T> where T : new()
    {
        IEnumerable<T> List();
    }
    public interface IRUDService<T> where T : new()
    {
        void Delete(Guid id);
        T Read(Guid id);
        T Update(T element);
    }
    public interface ICLService<T>: ICService<T>, ILService<T> where T : new()
    {     
      
    }
    //public interface ICRUDService<T> : ICService<T>, IRUDService<T> where T : new()
    //{
    //    //T Create(T todo);
    //    //void Delete();
    //    //T Read();
    //    //T Update(T todo);
    //}
  

    public interface ICRUDLService<T> : ICLService<T>, IRUDService<T> where T : new()
    {  
    }
}
