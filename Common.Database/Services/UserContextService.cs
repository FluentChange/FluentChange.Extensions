using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.Common.Database.Services
{
    public class UserContextService
    {
        [Obsolete("Use CurrentId instead")]
        public Guid? CurrentUserId => CurrentId;
        private Guid? currentId { get; set; }
        public Guid CurrentId
        {
            get
            {
                if (!currentId.HasValue) throw new ArgumentNullException();
                return currentId.Value;
            }
        }

        public void SetUser(Guid userId)
        {
            if (userId != null && userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));

            currentId = userId;
        }

        [Obsolete("Use EnsureExist() instead")]
        public void EnsureUser()
        {
            EnsureExist();
        }

        public void EnsureExist()
        {
            if (!currentId.HasValue || currentId.Value == Guid.Empty) throw new Exception("User is not set. Try to set user id in UserContext service");
        }

        public void Clear()
        {
            currentId = null;           
        }

        [Obsolete("Use Clear() instead")]
        public void ClearUser()
        {
            Clear();
        }
    }
}
