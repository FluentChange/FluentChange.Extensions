using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.Common.Database.Services
{
    public class UserContextService
    {
        [Obsolete("Use CurrentId instead")]
        public Guid? CurrentUserId => CurrentId;
        public Guid? CurrentId { get; private set; }

        public void SetUser(Guid userId)
        {
            if (userId != null && userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));

            CurrentId = userId;
        }

        [Obsolete("Use EnsureExist() instead")]
        public void EnsureUser()
        {
            EnsureExist();
        }

        public void EnsureExist()
        {
            if (!CurrentId.HasValue || CurrentId.Value == Guid.Empty) throw new Exception("User is not set. Try to set user id in UserContext service");
        }

        public void Clear()
        {
            CurrentId = null;           
        }

        [Obsolete("Use Clear() instead")]
        public void ClearUser()
        {
            Clear();
        }
    }
}
