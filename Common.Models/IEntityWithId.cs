using System;

namespace FluentChange.Extensions.Common.Models
{
    public interface IEntityWithId
    {
        Guid Id { get; set; }
    }
    public abstract class AbstractIdEntity : IEntityWithId
    {
        public Guid Id { get; set; }
    }
    public abstract class AbstractTrackedEntity : AbstractIdEntity
    {
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }

    public abstract class AbstractUserTrackedEntity : AbstractTrackedEntity
    {
        public Guid CreatedById { get; set; }
        public Guid UpdatedById { get; set; }
    }
}
