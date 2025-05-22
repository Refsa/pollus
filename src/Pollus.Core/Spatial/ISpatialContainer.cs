namespace Pollus.Spatial;

using Pollus.Collections;
using Pollus.Mathematics;
using System.Runtime.CompilerServices;

public struct QueryResult<TData>
    where TData : unmanaged
{
    public TData Data;
    public float DistanceSqr;
}

public interface ISpatialContainer<TData>
    where TData : unmanaged
{
    void Clear();
    void Prepare();

    void Insert(TData data, Vec2f position, float radius, uint layer);
    int Query(Vec2f position, float radius, uint layer, Span<TData> results);

    void Insert<TLayer>(TData entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum;
    int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<TData> results) where TLayer : unmanaged, Enum;
}

public static class SpatialQueryUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void TryInsert<TData>(scoped in Span<QueryResult<TData>> results, ref int cursor, in TData data, float distanceSqr)
        where TData : unmanaged
    {
        if (cursor < results.Length)
        {
            var insertIndex = cursor;
            while (insertIndex > 0 && results[insertIndex - 1].DistanceSqr > distanceSqr)
            {
                results[insertIndex] = results[insertIndex - 1];
                insertIndex--;
            }
            results[insertIndex] = new() { Data = data, DistanceSqr = distanceSqr };
            cursor++;
            return;
        }

        if (distanceSqr >= results[cursor - 1].DistanceSqr) return;

        var i = cursor - 1;
        while (i > 0 && results[i - 1].DistanceSqr > distanceSqr)
        {
            results[i] = results[i - 1];
            i--;
        }
        results[i] = new() { Data = data, DistanceSqr = distanceSqr };
    }
}