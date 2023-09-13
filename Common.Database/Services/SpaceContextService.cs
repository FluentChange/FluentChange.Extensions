using System;

namespace FluentChange.Extensions.Common.Database.Services
{
    public abstract class AbstractContextService
    {
        protected abstract String name { get; }

        private Guid? currentId { get; set; }
        public Guid CurrentId
        {
            get
            {
                if (!currentId.HasValue) throw new ArgumentNullException();
                return currentId.Value;
            }
        }

        public void Set(Guid spaceId)
        {
            if (spaceId == Guid.Empty) throw new ArgumentNullException(nameof(spaceId));

            currentId = spaceId;
        }

        public void EnsureExist()
        {
            if (!currentId.HasValue || currentId.Value == Guid.Empty) throw new Exception(name + "Id is not set. Try to set space id in " + name + "Context service");
        }

        public void Clear()
        {
            currentId = null;
        }
    }
    public class SpaceContextService : AbstractContextService
    {
        //private Guid? currentId { get; set; }
        //public Guid CurrentId { get
        //    {
        //        if (!currentId.HasValue) throw new ArgumentNullException();
        //        return currentId.Value;
        //    }
        //}

        //public void Set(Guid spaceId)
        //{
        //    if (spaceId != null && spaceId == Guid.Empty) throw new ArgumentNullException(nameof(spaceId));

        //    currentId = spaceId;
        //}

        //public void EnsureExist()
        //{
        //    if (!currentId.HasValue || currentId.Value == Guid.Empty) throw new Exception("Space is not set. Try to set space id in SpaceContext service");
        //}

        //public void Clear()
        //{
        //    currentId = null;
        //}
        protected override string name => "Space";
    }
    public class ClientContextService : AbstractContextService
    {
        protected override string name => "Client";
    }
    public class TenantContextService : AbstractContextService
    {
        protected override string name => "Tenant";
    }
    public class UserContextService : AbstractContextService
    {
        protected override string name => "User";
    }
}
