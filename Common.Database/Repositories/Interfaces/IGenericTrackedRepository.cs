using FluentChange.Extensions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database.Repositories.Interfaces
{
    public interface IGenericTrackedRepository<TEntity> where TEntity : IEntityWithId
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<bool> DeleteAsync(IEnumerable<TEntity> entities);
        Task<bool> DeleteAsync(TEntity entity);
        TEntity? GetById(Guid id);
        Task<TEntity?> GetByIdAsync(Guid id);
        IQueryable<TEntity> List();
        Task<IQueryable<TEntity>> ListAsync();
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);
    }
}