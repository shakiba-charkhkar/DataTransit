using AutoMapper;

namespace RedisDataTransit.Models
{
    public class DataTransitMapperProfile : Profile
    {
        public DataTransitMapperProfile()
        {
            CreateMap<DataTransit, DataTransitViewModel>();
        }
    }
}
