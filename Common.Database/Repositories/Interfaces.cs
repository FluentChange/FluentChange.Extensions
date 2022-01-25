using FluentChange.Extensions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database
{
    public interface IxRepository<T, E> where T : E
    {
        IQueryable<T> All();

        T GetById(Guid id);
        Task<T> GetByIdAsync(Guid id);

        void Insert(T entity);
        Task InsertAsync(T entity);
        void InsertBulk(IEnumerable<T> entity);
        Task InsertBulkAsync(IEnumerable<T> entity);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void Delete(Guid id);
        void Delete(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(T entity);
        void DeleteBulk(IEnumerable<T> entities);

        bool Exist(Guid id);
        Task<bool> ExistAsync(Guid id);
    }
    public interface ITrackedRepository<T> : IxRepository<T, AbstractTrackedEntity> where T : AbstractTrackedEntity
    {

    }
    public interface IUserTrackedRepository<T> : IxRepository<T, AbstractUserTrackedEntity> where T : AbstractUserTrackedEntity
    {

    }
}
