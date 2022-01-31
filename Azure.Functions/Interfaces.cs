using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Azure.Functions.Interfaces
{
    public interface ICService<T> where T : class
    {
        T Create(T entity);
    }
    public interface ILService<T> where T : class
    {
        IEnumerable<T> List();
    }
    public interface IRUDServiceWithId<T> where T : class
    {
        void Delete(Guid id);
        T Read(Guid id);
        T Update(T entity);
    }
    public interface IRUDServiceWithoutId<T> where T : class
    {
        void Delete();
        T Read();
        T Update(T entity);
    }
    public interface ICLService<T> : ICService<T>, ILService<T> where T : class
    {

    }

    public interface ICRUDLServiceWithId<T> : ICLService<T>, IRUDServiceWithId<T> where T : class
    {
    }
  
    public interface ICRUDServiceWithoutId<T> : ICService<T>, IRUDServiceWithoutId<T> where T : class
    {
    }
    public interface ICRUDServiceWithId<T> : ICService<T>, IRUDServiceWithId<T> where T : class
    {
    }
}
