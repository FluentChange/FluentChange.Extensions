using System;

namespace FluentChange.Extensions.Common.Database.Services
{

    public class ContextService
    {
        private readonly TenantContextService tenantContext;
        private readonly UserContextService userContext;
        private readonly ClientContextService clientContext;
        private readonly SpaceContextService spaceContext;

        public Guid? CurrentTenantId => tenantContext.CurrentId;
        public Guid? CurrentUserId => userContext.CurrentId;
        public Guid? CurrentClientId => clientContext.CurrentId;
        public Guid? CurrentSpaceId => spaceContext.CurrentId;

        public ContextService(UserContextService userContext, TenantContextService tenantContext, ClientContextService clientContext, SpaceContextService spaceContext)
        {
            this.userContext = userContext;
            this.spaceContext = spaceContext;
            this.tenantContext = tenantContext;
            this.clientContext = clientContext;
        }

        public void SetTenant(Guid userId) => tenantContext.Set(userId);
        public void SetUser(Guid userId) => userContext.Set(userId);
        public void SetClient(Guid clientId) => clientContext.Set(clientId);
        public void SetSpace(Guid spaceId) => spaceContext.Set(spaceId);

        public void EnsureTenant() => tenantContext.EnsureExist();
        public void EnsureUser() => userContext.EnsureExist();
        public void EnsureClient()=> clientContext.EnsureExist();
        public void EnsureSpace()=> spaceContext.EnsureExist();

        public void ClearTenant() => tenantContext.Clear();
        public void ClearUser() => userContext.Clear();
        public void ClearClient() => clientContext.Clear();
        public void ClearSpace() => spaceContext.Clear();

    }
}