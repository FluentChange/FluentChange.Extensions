using FluentChange.Extensions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database.Repositories.Interfaces
{
    [Obsolete("Please use SmartRepo")]
    public interface IRepository<T, E> : IRepository where T : E where E : AbstractIdEntity
    {
        IQueryable<T> All();
        T GetById(Guid id);
        Task<T> GetByIdAsync(Guid id);

        void Insert(T entity);
        Task InsertAsync(T entity);
        void InsertBulk(IEnumerable<T> entity);
        Task InsertBulkAsync(IEnumerable<T> entity);

        [Obsolete("use UpdateSave")]
        void Update(T entity);
        [Obsolete("use UpdateuSaveAsync")]
        Task UpdateAsync(T entity);

        void UpdateSave(T entity);
        Task UpdateSaveAsync(T entity);

        void Delete(Guid id);
        void Delete(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(T entity);
        void DeleteBulk(IEnumerable<T> entities);

        bool Exist(Guid id);
        Task<bool> ExistAsync(Guid id);

        void Reload(T entity);
        Task ReloadAsync(T entity, CancellationToken cancellationToken);

        void Detach(T entity);

        X GetOldValue<X>(T entity, string propertyName);
    }
}
