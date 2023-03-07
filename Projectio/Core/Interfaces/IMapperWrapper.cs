namespace Projectio.Core.Interfaces
{
    public interface IMapperWrapper
    {
        T2 Map<T, T2>(T object1) where T : class where T2 : class;

    }
}
