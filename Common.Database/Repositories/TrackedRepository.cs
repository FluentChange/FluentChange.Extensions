
using FluentChange.Extensions.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database
{

    public class TrackedRepository<E, D> : ITrackedRepository<E> where E : AbstractTrackedEntity where D : DbContext
    {
        protected readonly D database;
        private DbSet<E> dbSet;
        string errorMessage = string.Empty;
        private bool allowInsertWithNewId = false;
        public TrackedRepository(D database, bool allowInsertWithNewId = false)
        {
            this.database = database;
            dbSet = database.Set<E>();
            this.allowInsertWithNewId = allowInsertWithNewId;
        }

        public virtual IQueryable<E> All()
        {
            return dbSet;
        }

        public virtual E GetById(Guid id)
        {
            return dbSet.Find(id);
        }
        public virtual async Task<E> GetByIdAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual void Insert(E entity)
        {

            if (entity == null) throw new ArgumentNullException("entity");
            if (!allowInsertWithNewId && entity.Id != Guid.Empty) throw new Exception("Can not add existing entity.");
            var now = DateTime.UtcNow;
            entity.CreatedUtc = now;
            //entity.UpdatedUtc = now;
            dbSet.Add(entity);
            database.SaveChanges();
        }
        public virtual void InsertBulk(IEnumerable<E> entities)
        {
            if (entities == null) throw new ArgumentNullException("entities");
            foreach (var entity in entities)
            {
                if (!allowInsertWithNewId && entity.Id != Guid.Empty) throw new Exception("Can not add existing entity.");
                var now = DateTime.UtcNow;
                entity.CreatedUtc = now;
                //entity.UpdatedUtc = now;
                //    dbSet.Add(entity);
            }
            database.AddRange(entities.ToList());
            database.SaveChanges();
        }
        public virtual async Task InsertAsync(E entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!allowInsertWithNewId && entity.Id != Guid.Empty) throw new Exception("Can not add existing entity.");
            var now = DateTime.UtcNow;
            entity.CreatedUtc = now;
            //entity.UpdatedUtc = now;

            await dbSet.AddAsync(entity);
            await database.SaveChangesAsync();
        }

        public virtual async Task InsertBulkAsync(IEnumerable<E> entities)
        {
            if (entities == null) throw new ArgumentNullException("entity");
            var tasks = new List<ValueTask>();
            foreach (var entity in entities)
            {
                if (!allowInsertWithNewId && entity.Id != Guid.Empty) throw new Exception("Can not add existing entity.");
                var now = DateTime.UtcNow;
                entity.CreatedUtc = now;
                //entity.UpdatedUtc = now;
                //    await dbSet.AddAsync(entity);
            }
            await database.AddRangeAsync(entities.ToList());
            await database.SaveChangesAsync();

        }

        public virtual void Update(E entity)
        {
            UpdateSave(entity);
        }
        public virtual void UpdateSave(E entity)
        {
            // for direct calls i.e. in unit tests without rest we need to detach 
            Detach(entity);

            if (entity == null) throw new ArgumentNullException("entity");
            entity.UpdatedUtc = DateTime.UtcNow;
            //database.Attach(entity).State = EntityState.Modified;
            var result = dbSet.Update(entity);
            database.SaveChanges();
        }
        public virtual async Task UpdateAsync(E entity)
        {
           await UpdateSaveAsync(entity);
        }
        public virtual async Task UpdateSaveAsync(E entity)
        {
            // for direct calls i.e. in unit tests without rest we need to detach 
            Detach(entity);

            if (entity == null) throw new ArgumentNullException("entity");
            entity.UpdatedUtc = DateTime.UtcNow;
            //database.Attach(entity).State = EntityState.Modified;
            var result = dbSet.Update(entity);
            await database.SaveChangesAsync();
        }

        public virtual void Delete(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("id");

            E entity = dbSet.Find(id);
            if (entity != null)
            {
                Delete(entity);
            }           
        }
        public virtual void Delete(E entity)
        {
            dbSet.Remove(entity);
            database.SaveChanges();
        }
        public virtual async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));

            E entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }
        public virtual async Task DeleteAsync(E entity)
        {
            dbSet.Remove(entity);
            await database.SaveChangesAsync();
        }
        public void DeleteBulk(IEnumerable<E> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                dbSet.Remove(entity);
            }
            database.SaveChanges();
        }

        public virtual bool Exist(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("id");
            return dbSet.Any(e => e.Id == id);
        }
        public virtual async Task<bool> ExistAsync(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("id");
            return await dbSet.AnyAsync(e => e.Id == id);
        }

        public void Reload(E entity)
        {
            database.Entry(entity).Reload();
        }
        public virtual async Task ReloadAsync(E entity, CancellationToken cancellationToken)
        {
            await database.Entry(entity).ReloadAsync(cancellationToken);
        }        
 
        public void Detach(E entity)
        {
            var localTrackedEntity = database.Set<E>().Local.FirstOrDefault(entry => entry.Id.Equals(entity.Id));
            if (localTrackedEntity != null)
            {
                database.Entry(localTrackedEntity).State = EntityState.Detached;
            }
        }

        public X GetOldValue<X>(E entity, string propertyName)
        {
            var allChanges = database.ChangeTracker.Entries<E>();
            var original = allChanges.Single(x => x.Entity.Id == entity.Id);
            var originalValue = original.Property<X>(propertyName).OriginalValue;
            return originalValue;
        }

    }
}
