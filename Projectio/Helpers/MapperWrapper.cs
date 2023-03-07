using AutoMapper;
using Projectio.Core.Interfaces;

namespace Projectio.Helpers
{
    public class MapperWrapper : IMapperWrapper
    {
        public T2 Map<T, T2>(T object1)
            where T : class
            where T2 : class
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<T, T2>());
            var mapper = new Mapper(config);
            return mapper.Map<T2>(object1);
        }
    }
}
