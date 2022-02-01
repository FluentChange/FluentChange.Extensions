using System.Linq;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    public interface IEntityMapper
    {
        M MapTo<M>(object source);
        IQueryable<M> ProjectTo<M>(IQueryable source);
    }
}
