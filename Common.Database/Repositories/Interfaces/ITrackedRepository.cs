using FluentChange.Extensions.Common.Models;
using System;

namespace FluentChange.Extensions.Common.Database.Repositories.Interfaces
{
    [Obsolete("Please use SmartRepo")]
    public interface ITrackedRepository<T> : IRepository<T, AbstractTrackedEntity> where T : AbstractTrackedEntity
    {

    }
}
