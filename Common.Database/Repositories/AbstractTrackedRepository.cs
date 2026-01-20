using FluentChange.Extensions.Common.Database.Repositories.Interfaces;
using FluentChange.Extensions.Common.Database.Services.Interfaces;
using FluentChange.Extensions.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Common.Database
{
    public class AbstractTrackedRepository(IUserContextService userContext, ILogger logger) : IRepository
    {
        protected void TrackAdd<T>(T trackedModel) where T : ITrackedModel
        {
            EnsureUserContext();
            trackedModel.CreatedUtc = DateTime.UtcNow;
            trackedModel.CreatedById = userContext.Current()!.Value;
        }
        protected void TrackAdd<T>(IEnumerable<T> trackedModels) where T : ITrackedModel
        {
            EnsureUserContext();
            var currentUserId = userContext.Current()!.Value;
            foreach (var trackedModel in trackedModels)
            {
                trackedModel.CreatedUtc = DateTime.UtcNow;
                trackedModel.CreatedById = currentUserId;
            }
        }

        protected void TrackUpdate<T>(T trackedModel) where T : ITrackedModel
        {
            EnsureUserContext();
            trackedModel.UpdatedUtc = DateTime.UtcNow;
            trackedModel.UpdatedById = userContext.Current()!.Value;
        }
        protected void TrackUpdate<T>(IEnumerable<T> trackedModels) where T : ITrackedModel
        {
            EnsureUserContext();
            var currentUserId = userContext.Current()!.Value;
            foreach (var trackedModel in trackedModels)
            {
                trackedModel.UpdatedUtc = DateTime.UtcNow;
                trackedModel.UpdatedById = currentUserId;
            }
        }
        private void EnsureUserContext()
        {
            if (userContext.Current() is null)
            {
                var exception = new InvalidOperationException("Valid user context is needed.");
                logger.LogError(exception, "Operation failed due to missing user context.");
                throw exception;
            }
        }

        protected void TrackDelete<T>(T trackedModel) where T : ITrackedModel
        {
            // nothing todo here, as we do not support soft deletes yet
        }

        protected void TrackDelete<T>(IEnumerable<T> trackedModels) where T : ITrackedModel
        {
            // nothing todo here, as we do not support soft deletes yet
        }
    }
}
