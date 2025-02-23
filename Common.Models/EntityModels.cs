using System;

namespace FluentChange.Extensions.Common.Models
{
    public interface IEntity
    {

    }
    public interface IEntityWithId : IEntity
    {
        Guid Id { get; set; }
    }

    public interface ITimeTrackedEntity
    {
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
    }
    public interface ITrackedEntity
    {
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
    public interface IUserTrackedEntity
    {
        public Guid CreatedById { get; set; }
        public Guid UpdatedById { get; set; }
    }
    public interface ISpaceDependendEntity
    {
        public Guid SpaceId { get; set; }      
    }


    public abstract class AbstractEntity : IEntity
    {

    }
    public abstract class AbstractIdEntity : AbstractEntity, IEntityWithId
    {
        public Guid Id { get; set; }
    }
    public abstract class AbstractTrackedEntity : AbstractIdEntity, ITrackedEntity
    {
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
    public abstract class AbstractSpaceTrackedEntity : AbstractTrackedEntity, ISpaceDependendEntity
    {
        public Guid SpaceId { get; set; }
    }

    public abstract class AbstractUserTrackedEntity : AbstractTrackedEntity, IUserTrackedEntity
    {
        public Guid CreatedById { get; set; }
        public Guid UpdatedById { get; set; }
    }

    public abstract class AbstractSpaceUserTrackedEntity : AbstractUserTrackedEntity, ISpaceDependendEntity
    {
        public Guid SpaceId { get; set; }
    }
}
