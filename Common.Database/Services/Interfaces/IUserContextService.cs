using System;

namespace FluentChange.Extensions.Common.Database.Services.Interfaces
{
    public interface IUserContextService
    {
        public Guid? Current();
        public void Set(Guid userId);
    }

}
