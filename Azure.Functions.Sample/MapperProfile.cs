using AutoMapper;
using DemoCRUDLFunctions.Models;

namespace DemoCRUDLFunctions
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ApiProduct, Product>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(d => d.Description, opt => opt.MapFrom(src => src.Text))
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ReverseMap();

        }
    }
}
