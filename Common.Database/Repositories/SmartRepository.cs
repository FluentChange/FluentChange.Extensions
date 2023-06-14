﻿using EFCore.BulkExtensions;
using FluentChange.Extensions.Common.Database.Services;
using FluentChange.Extensions.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database
{

    public class SmartRepository<E, D> : ISmartRepository<E> where E : AbstractEntity where D : DbContext
    {
        protected readonly D database;
        private DbSet<E> dbSet;
        private UserContextService contextUser;
        private SpaceContextService contextSpace;

        string errorMessage = string.Empty;
        private bool allowInsertWithNewId = false;
        private static Type eType = typeof(E);
        private static bool isIdEntity = typeof(IEntityWithId).IsAssignableFrom(eType);
        private static bool isTrackedEntity = typeof(ITrackedEntity).IsAssignableFrom(eType);
        private static bool isUserTrackedEntity = typeof(IUserTrackedEntity).IsAssignableFrom(eType);
        private static bool iSpaceDependendEntity = typeof(ISpaceDependendEntity).IsAssignableFrom(eType);
     

        public SmartRepository(D database, UserContextService userContext, SpaceContextService contextSpace, bool allowInsertWithNewId = false)
        {
            this.database = database;
            dbSet = database.Set<E>();
            this.allowInsertWithNewId = allowInsertWithNewId;
            this.contextUser = userContext;
            this.contextSpace = contextSpace;
        }

        #region READ

        public virtual IQueryable<E> All()
        {
            return dbSet;
        }
        public virtual E GetById(Guid id)
        {
            CheckIfIdSupported();
            return dbSet.Find(id);
        }
        public virtual async Task<E> GetByIdAsync(Guid id)
        {
            CheckIfIdSupported();
            return await dbSet.FindAsync(id);
        }

        #endregion

        #region INSERT
        public virtual void Insert(E entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            InsertCheckAndTrack(entity);

            dbSet.Add(entity);
            database.SaveChanges();
        }
            

        public virtual void InsertBulk(IEnumerable<E> entities)
        {
            if (entities == null) throw new ArgumentNullException("entities");
            foreach (var entity in entities)
            {
                InsertCheckAndTrack(entity);
                //    dbSet.Add(entity);
            }
            database.BulkInsert(entities.ToList());
            database.SaveChanges();
        }
        public virtual async Task InsertAsync(E entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            InsertCheckAndTrack(entity);

            await dbSet.AddAsync(entity);
            await database.SaveChangesAsync();
        }

        public virtual async Task InsertBulkAsync(IEnumerable<E> entities)
        {
            if (entities == null) throw new ArgumentNullException("entity");
            var tasks = new List<ValueTask>();
            foreach (var entity in entities)
            {
                InsertCheckAndTrack(entity);
                //    await dbSet.AddAsync(entity);
            }
            await database.BulkInsertAsync(entities.ToList());
            await database.SaveChangesAsync();

        }

        private void InsertCheckAndTrack(E entity)
        {
            CheckForIdInsert(entity);
            TrackDateCreatedIfNeeded(entity);
            TrackUserCreatedIfNeeded(entity);
            TrackSpaceIfNeeded(entity);
        }

        #endregion

        #region UPDATE

        public virtual void Update(E entity)
        {
            UpdateSave(entity);
        }
        public virtual void UpdateSave(E entity)
        {
            // for direct calls i.e. in unit tests without rest we need to detach 
            Detach(entity);

            if (entity == null) throw new ArgumentNullException("entity");
            UpdateCheckAndTrack(entity);
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
            UpdateCheckAndTrack(entity);
            //database.Attach(entity).State = EntityState.Modified;
            var result = dbSet.Update(entity);
            await database.SaveChangesAsync();
        }
        private void UpdateCheckAndTrack(E entity)
        {
            TrackDateUpdatedIfNeeded(entity);
            TrackUserUpdatedIfNeeded(entity);
            CheckSpaceIfNeeded(entity);  
        }

        #endregion

        #region DELETE

        public virtual void Delete(Guid id)
        {
            CheckIfIdSupported();
            if (id == null || id == Guid.Empty) throw new ArgumentNullException("id");

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
            CheckIfIdSupported();
            if (id == null || id == Guid.Empty) throw new ArgumentNullException(nameof(id));

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

        #endregion

        #region HELPERS
        public virtual bool Exist(Guid id)
        {
            CheckIfIdSupported();
            if (id == null || id == Guid.Empty) throw new ArgumentNullException("id");
            return dbSet.Any(e => ((IEntityWithId)e).Id == id);
        }
        public virtual async Task<bool> ExistAsync(Guid id)
        {
            CheckIfIdSupported();
            if (id == null || id == Guid.Empty) throw new ArgumentNullException("id");
            return await dbSet.AnyAsync(e => ((IEntityWithId)e).Id == id);
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
            CheckIfIdSupported();
            var localTrackedEntity = database.Set<E>().Local.FirstOrDefault(entry => ((IEntityWithId)entry).Id.Equals(((IEntityWithId)entity).Id));
            if (localTrackedEntity != null)
            {
                database.Entry(localTrackedEntity).State = EntityState.Detached;
            }
        }

        public X GetOldValue<X>(E entity, string propertyName)
        {
            CheckIfIdSupported();
            var allChanges = database.ChangeTracker.Entries<E>();
            var original = allChanges.Single(x => ((IEntityWithId)x.Entity).Id == ((IEntityWithId)entity).Id);
            var originalValue = original.Property<X>(propertyName).OriginalValue;
            return originalValue;
        }
        #endregion

        #region INTERNAL HELPERS
        private void CheckIfIdSupported()
        {
            if (!isIdEntity) throw new Exception("Entity do not support id");

        }
        private void CheckForIdInsert(E entity)
        {
            if (isIdEntity)
            {
                if (!allowInsertWithNewId && ((IEntityWithId)entity).Id != Guid.Empty) throw new Exception("Can not add existing entity.");
            }
        }

        private void TrackDateCreatedIfNeeded(E entity)
        {
            if (isTrackedEntity)
            {
                var now = DateTime.UtcNow;
                ((ITrackedEntity)entity).CreatedUtc = now;
                //((ITrackedEntity)entity).UpdatedUtc = now;
            }
        }
        private void TrackUserCreatedIfNeeded(E entity)
        {
            if (isTrackedEntity)
            {
                contextUser.EnsureExist();
                if (((IUserTrackedEntity)entity).CreatedById != Guid.Empty
                    && ((IUserTrackedEntity)entity).CreatedById != contextUser.CurrentId.Value)
                {
                    throw new ArgumentException("CreatedById should not be set or has to be same as current user");
                }
                ((IUserTrackedEntity)entity).CreatedById = contextUser.CurrentId.Value;
                //((IUserTrackedEntity)entity).UpdatedUtc = userContext.CurrentUserId.Value;
            }
        }
        private void TrackDateUpdatedIfNeeded(E entity)
        {
            if (isUserTrackedEntity)
            {
                ((ITrackedEntity)entity).UpdatedUtc = DateTime.UtcNow;
            }
        }
        private void TrackUserUpdatedIfNeeded(E entity)
        {
            if (isUserTrackedEntity)
            {
                contextUser.EnsureExist();
                if (((IUserTrackedEntity)entity).UpdatedById != Guid.Empty
                    && ((IUserTrackedEntity)entity).UpdatedById != contextUser.CurrentId.Value)
                {
                    throw new ArgumentException("UpdatedById should not be set or has to be same as current user");
                }               
                ((IUserTrackedEntity)entity).UpdatedById = contextUser.CurrentId.Value;
            }
        }

        private void TrackSpaceIfNeeded(E entity)
        {
            if (iSpaceDependendEntity)
            {
                contextSpace.EnsureExist();
                if (((ISpaceDependendEntity)entity).SpaceId != Guid.Empty
                   && ((ISpaceDependendEntity)entity).SpaceId != contextSpace.CurrentId.Value)
                {
                    throw new ArgumentException("SpaceId should not be set or has to be same as current space");
                }
                ((ISpaceDependendEntity)entity).SpaceId = contextSpace.CurrentId.Value;
            }
        }
        private void CheckSpaceIfNeeded(E entity)
        {
            if (iSpaceDependendEntity)
            {
                contextSpace.EnsureExist();
                if (((ISpaceDependendEntity)entity).SpaceId != Guid.Empty
                   && ((ISpaceDependendEntity)entity).SpaceId != contextSpace.CurrentId.Value)
                {
                    throw new ArgumentException("SpaceId should not be set or has to be same as current space");
                }               
            }
        }
        #endregion

    }
}
