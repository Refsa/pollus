namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Mathematics;

public interface ISpatialQuery
{
    void Clear();

    void Insert(Entity entity, Vec2f position, float radius, uint layer);
    int Query(Vec2f position, float radius, uint layer, Span<Entity> results);

    void Insert<TLayer>(Entity entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum;
    int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<Entity> results) where TLayer : unmanaged, Enum;
}

public class SpatialQuery : ISpatialQuery
{
    ISpatialQuery query;

    public SpatialQuery(ISpatialQuery query)
    {
        this.query = query;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        query.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(Entity entity, Vec2f position, float radius, uint layer)
    {
        query.Insert(entity, position, radius, layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query(Vec2f position, float radius, uint layer, Span<Entity> results)
    {
        return query.Query(position, radius, layer, results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(Entity entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        query.Insert(entity, position, radius, layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<Entity> results) where TLayer : unmanaged, Enum
    {
        return query.Query(position, radius, layer, results);
    }
}

public class SpatialGrid : ISpatialQuery
{
    SpatialHashGrid<Entity> cache;

    public SpatialGrid(int cellSize, int width, int height)
    {
        cache = new SpatialHashGrid<Entity>(cellSize, width, height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        cache.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(Entity entity, Vec2f position, float radius, uint layer)
    {
        cache.Insert(entity, position, radius, layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(Entity entity, Vec2f position, float radius, TLayer layer)
        where TLayer : unmanaged, Enum
    {
        cache.Insert(entity, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query(Vec2f position, float radius, uint layer, Span<Entity> results)
    {
        return cache.Query(position, radius, layer, results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<Entity> results)
        where TLayer : unmanaged, Enum
    {
        return cache.Query(position, radius, Unsafe.As<TLayer, uint>(ref layer), results);
    }
}