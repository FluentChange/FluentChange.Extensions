using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.Common.Database.Services
{
    public class UserContextService
    {
        public Guid? CurrentUserId { get; private set; }

        public void SetUser(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentOutOfRangeException();

            CurrentUserId = userId;
        }

        public void EnsureUser()
        {
            if (!CurrentUserId.HasValue || CurrentUserId.Value == Guid.Empty) throw new Exception("User is not set. Try to set user id in UserContext service");
        }

        public void ClearUser()
        {
            CurrentUserId = null;
        }
    }
}
