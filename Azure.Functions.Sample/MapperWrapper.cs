using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentChange.Extensions.Azure.Functions.Helper;
using System.Linq;

namespace DemoCRUDLFunctions
{
    public class MapperWrapper: IEntityMapper
    {
        private readonly IMapper automapper;
        public MapperWrapper(IMapper automapper)
        {
            this.automapper = automapper;
        }
        public IQueryable<M> ProjectTo<M>(IQueryable source)
        {
            return source.ProjectTo<M>(automapper.ConfigurationProvider);
        }

        public M MapTo<M>(object source)
        {
            return automapper.Map<M>(source);
        }
    }
}
