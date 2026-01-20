using FluentChange.Extensions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database.Repositories.Interfaces
{
    public interface ISmartRepository<T> : IRepository where T : IEntity
    {
        IQueryable<T> All();
        T GetById(Guid id);
        Task<T> GetByIdAsync(Guid id);

        void Insert(T entity);
        Task InsertAsync(T entity);
        void InsertBulk(IEnumerable<T> entity, bool allowInsertId = false);
        Task InsertBulkAsync(IEnumerable<T> entity);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void UpdateSave(T entity);
        Task UpdateSaveAsync(T entity);

        void Delete(Guid id);
        void DeleteSave(T entity);
        Task DeleteSaveAsync(Guid id);
        Task DeleteSaveAsync(T entity);
        void DeleteSave(IEnumerable<T> entities);

        bool Exist(Guid id);
        Task<bool> ExistAsync(Guid id);

        void Reload(T entity);
        Task ReloadAsync(T entity, CancellationToken cancellationToken);

        void Detach(T entity);

        X GetOldValue<X>(T entity, string propertyName);
    }
}
