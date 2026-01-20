using FluentChange.Extensions.Common.Database.Repositories.Interfaces;
using FluentChange.Extensions.Common.Database.Services.Interfaces;
using FluentChange.Extensions.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database
{
    public abstract class GenericTrackedRepository<TEntity>(DbContext dbContext, IUserContextService userContext, ILogger logger)
        : AbstractTrackedRepository(userContext, logger), IRepository, IGenericTrackedRepository<TEntity> where TEntity : class, IEntityWithId, ITrackedModel
    {

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            TrackAdd(entity);

            await dbContext.Set<TEntity>().AddAsync(entity);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Added entity of type {EntityType} with Id {EntityId}", typeof(TEntity).Name, entity.Id);

            return entity;
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            TrackAdd(entities);

            await dbContext.Set<TEntity>().AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Added {EntityCount} entities of type {EntityType}", entities.Count(), typeof(TEntity).Name);

            return entities;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            // Check if entity is already being tracked to avoid tracking conflicts
            var trackedEntity = dbContext.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);
            if (trackedEntity != null)
            {
                // Update the values of the tracked entity
                TrackUpdate(trackedEntity);
                dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                TrackUpdate(entity);
                dbContext.Set<TEntity>().Update(entity);
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Updated entity of type {EntityType} with Id {EntityId}", typeof(TEntity).Name, entity.Id);

            return trackedEntity ?? entity;
        }
        public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            TrackUpdate(entities);

            dbContext.Set<TEntity>().UpdateRange(entities);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Updated {EntityCount} entities of type {EntityType}", entities.Count(), typeof(TEntity).Name);

            return entities;
        }
        public async Task<bool> DeleteAsync(TEntity entity)
        {
            TrackDelete(entity);

            dbContext.Set<TEntity>().Remove(entity);
            var changes = await dbContext.SaveChangesAsync();

            logger.LogInformation("Deleted entity of type {EntityType} with Id {EntityId}", typeof(TEntity).Name, entity.Id);

            return changes == 1;
        }

        public async Task<bool> DeleteAsync(IEnumerable<TEntity> entities)
        {
            TrackDelete(entities);

            dbContext.Set<TEntity>().RemoveRange(entities);
            var changes = await dbContext.SaveChangesAsync();

            logger.LogInformation("Deleted {EntityCount} entities of type {EntityType}", entities.Count(), typeof(TEntity).Name);

            return changes == entities.Count();
        }

        public TEntity? GetById(Guid id)
        {
            logger.LogInformation("Retrieving entity of type {EntityType} with Id {EntityId}", typeof(TEntity).Name, id);

            return List().FirstOrDefault(o => o.Id == id);
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            logger.LogInformation("Retrieving entity of type {EntityType} with Id {EntityId}", typeof(TEntity).Name, id);

            return await List().FirstOrDefaultAsync(o => o.Id == id);
        }

        public IQueryable<TEntity> List()
        {
            logger.LogInformation("Listing entities of type {EntityType}", typeof(TEntity).Name);

            return dbContext.Set<TEntity>();
        }

        public async Task<IQueryable<TEntity>> ListAsync()
        {
            logger.LogInformation("Listing entities of type {EntityType}", typeof(TEntity).Name);

            return dbContext.Set<TEntity>();
        }
    }
}
