using FluentChange.Extensions.Common.Database.Services;
using System;

namespace FluentChange.Extensions.Common.Database.Services
{

    public class ContextService
    {
        private readonly UserContextService userContextService;
        private readonly SpaceContextService spaceContextService;
        public bool IsAuth { get; private set; }
        public Guid? CurrentUserId => userContextService.CurrentId;

        public Guid? CurrentClientId { get; private set; }
        public Guid? CurrentSpaceId => spaceContextService.CurrentId;

        public string CurrentClientSlug { get; private set; }
        public string CurrentSpaceSlug { get; private set; }

        public ContextService(UserContextService userContextService, SpaceContextService spaceContextService)
        {
            this.userContextService = userContextService;
            this.spaceContextService = spaceContextService;
        }

        public void SetUser(Guid userId)
        {
            userContextService.Set(userId);
            IsAuth = true;
        }
        public void SetClient(Guid clientId)
        {
            if (clientId != null && clientId != Guid.Empty)
            {
                CurrentClientId = clientId;
            }
            else
            {
                throw new ArgumentNullException(nameof(clientId));
            }
        }
        public void SetClientSlug(string slug)
        {
            if (!String.IsNullOrEmpty(slug))
            {
                CurrentClientSlug = slug;
            }
            else
            {
                throw new ArgumentNullException(nameof(slug));
            }
        }
        public void SetSpace(Guid spaceId)
        {
            spaceContextService.Set(spaceId);
        }
        public void SetSpaceSlug(string slug)
        {
            if (!String.IsNullOrEmpty(slug))
            {
                CurrentSpaceSlug = slug;
            }
            else
            {
                throw new ArgumentNullException(nameof(slug));
            }
        }

        public void EnsureUser()
        {
            userContextService.EnsureExist();
        }
        public void EnsureClient()
        {
            if (!CurrentClientId.HasValue || CurrentClientId.Value == Guid.Empty) throw new Exception("please set client id");
            if (String.IsNullOrEmpty(CurrentClientSlug)) throw new Exception("please set client slug");
        }
        public void EnsureSpace()
        {
            spaceContextService.EnsureExist();
            if (String.IsNullOrEmpty(CurrentSpaceSlug)) throw new Exception("please set space slug");
        }

        public void ClearUser()
        {
            userContextService.Clear();
            IsAuth = false;   
        }
    }
}