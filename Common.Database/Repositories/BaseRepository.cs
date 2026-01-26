using FluentChange.Extensions.Common.Database.Repositories.Interfaces;
using FluentChange.Extensions.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database
{
    public abstract class BaseRepository<T> : IRepository where T : class, IEntityWithId
    {
        protected abstract DbSet<T> DbSet { get; }
        protected abstract Task<int> SaveChangesAsync();

        public virtual IQueryable<T> All()
        {
            return DbSet;
        }

        public virtual T? Get(Guid id)
        {
            return All().SingleOrDefault(t => t.Id == id);
        }
        public virtual async Task<T> AddAsync(T newEntity)
        {
            await DbSet.AddAsync(newEntity);
            await SaveChangesAsync();
            return newEntity;
        }

        public virtual async Task<T> UpdateAsync(T updatedEntity)
        {
            DbSet.Update(updatedEntity);
            await SaveChangesAsync();
            return updatedEntity;
        }
        public virtual async Task DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            await SaveChangesAsync();
        }
        public virtual async Task DeleteAsync(IEnumerable<T> entities)
        {
            DbSet.RemoveRange(entities);
            await SaveChangesAsync();
        }
        public virtual async Task DeleteAllAsync()
        {
            var entities = All();
            await DeleteAsync(entities);
        }

    }
}
