using FluentChange.Extensions.Common.Database.Services;
using FluentChange.Extensions.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Database.Repositories
{
    public class UserTrackedRepository<E, D> : TrackedRepository<E, D>, IUserTrackedRepository<E> where E : AbstractUserTrackedEntity where D : DbContext
    {
        protected UserContextService context;

        public UserTrackedRepository(D database, UserContextService context) : base(database)
        {
            this.context = context;
        }

        public override void Insert(E entity)
        {
            context.EnsureUser();
            entity.CreatedById = context.CurrentUserId.Value;
            base.Insert(entity);
        }
        public override async Task InsertAsync(E entity)
        {
            context.EnsureUser();
            entity.CreatedById = context.CurrentUserId.Value;
            await base.InsertAsync(entity);
        }

        public override void InsertBulk(IEnumerable<E> entities)
        {
            context.EnsureUser();
            if (entities == null) throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                entity.CreatedById = context.CurrentUserId.Value;
            }
            base.InsertBulk(entities);

        }
        public override async Task InsertBulkAsync(IEnumerable<E> entities)
        {
            context.EnsureUser();
            if (entities == null) throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                entity.CreatedById = context.CurrentUserId.Value;
            }
            await base.InsertBulkAsync(entities);
        }


        public override void UpdateSave(E entity)
        {
            context.EnsureUser();
            if (entity.UpdatedById != Guid.Empty && entity.UpdatedById != context.CurrentUserId.Value) throw new ArgumentException("UserId should not be set or same as current user");
            entity.UpdatedById = context.CurrentUserId.Value;
            base.UpdateSave(entity);
        }

        public override async Task UpdateSaveAsync(E entity)
        {
            context.EnsureUser();
            if (entity.UpdatedById != Guid.Empty && entity.UpdatedById != context.CurrentUserId.Value) throw new ArgumentException("UserId should not be set or same as current user");
            entity.UpdatedById = context.CurrentUserId.Value;
            await base.UpdateSaveAsync(entity);
        }

    }
}
