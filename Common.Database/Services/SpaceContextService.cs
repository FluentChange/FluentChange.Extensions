using System;

namespace FluentChange.Extensions.Common.Database.Services
{
    public class SpaceContextService
    {
        public Guid? CurrentId { get; private set; }

        public void Set(Guid spaceId)
        {
            if (spaceId != null && spaceId == Guid.Empty) throw new ArgumentNullException(nameof(spaceId));

            CurrentId = spaceId;
        }

        public void EnsureExist()
        {
            if (!CurrentId.HasValue || CurrentId.Value == Guid.Empty) throw new Exception("Space is not set. Try to set space id in SpaceContext service");
        }

        public void Clear()
        {
            CurrentId = null;
        }
    }
}
