using System;

namespace FluentChange.Extensions.Common.Database.Services
{
    public class SpaceContextService
    {
        private Guid? currentId { get; set; }
        public Guid CurrentId { get
            {
                if (!currentId.HasValue) throw new ArgumentNullException();
                return currentId.Value;
            }
        }

        public void Set(Guid spaceId)
        {
            if (spaceId != null && spaceId == Guid.Empty) throw new ArgumentNullException(nameof(spaceId));

            currentId = spaceId;
        }

        public void EnsureExist()
        {
            if (!currentId.HasValue || currentId.Value == Guid.Empty) throw new Exception("Space is not set. Try to set space id in SpaceContext service");
        }

        public void Clear()
        {
            currentId = null;
        }
    }
}
