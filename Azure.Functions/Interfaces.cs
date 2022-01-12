using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Azure.Functions.Interfaces
{
    public interface ICService<T> where T : new()
    {
        T Create(T entity);
    }
    public interface ILService<T> where T : new()
    {
        IEnumerable<T> List();
    }
    public interface IRUDServiceWithId<T> where T : new()
    {
        void Delete(Guid id);
        T Read(Guid id);
        T Update(T entity);
    }
    public interface IRUDServiceWithoutId<T> where T : new()
    {
        void Delete();
        T Read();
        T Update(T entity);
    }
    public interface ICLService<T> : ICService<T>, ILService<T> where T : new()
    {

    }

    public interface ICRUDLServiceWithId<T> : ICLService<T>, IRUDServiceWithId<T> where T : new()
    {
    }
  
    public interface ICRUDServiceWithoutId<T> : ICService<T>, IRUDServiceWithoutId<T> where T : new()
    {
    }
    public interface ICRUDServiceWithId<T> : ICService<T>, IRUDServiceWithId<T> where T : new()
    {
    }
}
