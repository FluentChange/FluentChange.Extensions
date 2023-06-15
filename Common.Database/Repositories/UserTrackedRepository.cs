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
        protected UserContextService contextUser;

        public UserTrackedRepository(D database, UserContextService context) : base(database)
        {
            this.contextUser = context;
        }

        public override void Insert(E entity)
        {
            contextUser.EnsureExist();
            entity.CreatedById = contextUser.CurrentId;
            base.Insert(entity);
        }
        public override async Task InsertAsync(E entity)
        {
            contextUser.EnsureExist();
            entity.CreatedById = contextUser.CurrentId;
            await base.InsertAsync(entity);
        }

        public override void InsertBulk(IEnumerable<E> entities)
        {
            contextUser.EnsureExist();
            if (entities == null) throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                entity.CreatedById = contextUser.CurrentId;
            }
            base.InsertBulk(entities);

        }
        public override async Task InsertBulkAsync(IEnumerable<E> entities)
        {
            contextUser.EnsureExist();
            if (entities == null) throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                entity.CreatedById = contextUser.CurrentId;
            }
            await base.InsertBulkAsync(entities);
        }


        public override void UpdateSave(E entity)
        {
            contextUser.EnsureExist();
            if (entity.UpdatedById != Guid.Empty && entity.UpdatedById != contextUser.CurrentId) throw new ArgumentException("UserId should not be set or same as current user");
            entity.UpdatedById = contextUser.CurrentId;
            base.UpdateSave(entity);
        }

        public override async Task UpdateSaveAsync(E entity)
        {
            contextUser.EnsureExist();
            if (entity.UpdatedById != Guid.Empty && entity.UpdatedById != contextUser.CurrentId) throw new ArgumentException("UserId should not be set or same as current user");
            entity.UpdatedById = contextUser.CurrentId;
            await base.UpdateSaveAsync(entity);
        }

    }
}
