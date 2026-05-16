#nullable enable
using FluentChange.Extensions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database.Repositories.Interfaces
{
    /// <summary>
    /// Generic CRUD contract that BaseRepository&lt;T&gt; satisfies. Per-entity
    /// repositories extend this and add entity-specific queries (e.g. GetBy(email)).
    /// </summary>
    public interface IBaseRepository<T> : IRepository where T : class, IEntityWithId
    {
        IQueryable<T> All();
        T? Get(Guid id);

        Task<T> AddAsync(T newEntity);
        Task<T> UpdateAsync(T updatedEntity);
        Task DeleteAsync(T entity);
        Task DeleteAsync(IEnumerable<T> entities);
        Task DeleteAllAsync();
    }
}
