namespace Pollus.Spatial;

using Pollus.Mathematics;

public interface ISpatialContainer<TData>
{
    void Clear();
    void Prepare();

    void Insert(TData data, Vec2f position, float radius, uint layer);
    int Query(Vec2f position, float radius, uint layer, Span<TData> results);

    void Insert<TLayer>(TData entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum;
    int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<TData> results) where TLayer : unmanaged, Enum;
}