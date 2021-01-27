using System;

namespace FluentChange.Extensions.Common.Rest
{
    public interface IEntityWithId
    {
        Guid Id { get; set; }
    }
}
