namespace Pollus.Spatial;

using System;
using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Mathematics;

/// <summary>
/// Single layer spatial has grid.
/// 
/// Works well for colliders with similar bounds
/// </summary>
/// <typeparam name="TData"></typeparam>
public class SpatialHashGrid<TData> : ISpatialContainer<TData>
    where TData : unmanaged
{
    public struct CellEntry
    {
        public TData Data;
        public uint Layer;
        public float Radius;
        public Vec2f Position;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly bool HasLayer(uint layer) => (Layer & layer) != 0;
    }

    public struct Cell
    {
        CellEntry[] entries = new CellEntry[1];
        int count;
        uint layerMask;

        public Span<CellEntry> Entries => entries.AsSpan(0, count);
        public int Count => count;

        public Cell() { }

        public void Clear()
        {
            count = 0;
            layerMask = 0;
        }

        public ref CellEntry this[int index] => ref entries[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Add(TData data, Vec2f position, uint layer, float radius)
        {
            layerMask |= layer;

            if (count >= entries.Length) Array.Resize(ref entries, count * 2);
            entries[count++] = new()
            {
                Data = data,
                Layer = layer,
                Radius = radius,
                Position = position
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly bool HasLayer(uint layer) => (layerMask & layer) != 0;
    }

    readonly int cellSize;
    readonly int width;
    readonly int height;

    Cell[] cells;
    float biggestRadius;

    public SpatialHashGrid(int cellSize, int width, int height)
    {
        this.cellSize = cellSize;
        this.width = width;
        this.height = height;

        cells = new Cell[width * height];
        for (int i = 0; i < cells.Length; i++) cells[i] = new Cell();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        biggestRadius = 0f;
        var span = cells.AsSpan();
        ref var curr = ref span[0];
        for (int i = 0; i < span.Length; i++, curr = ref Unsafe.Add(ref curr, 1))
        {
            curr.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(TData data, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        Insert(data, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(TData data, Vec2f position, float radius, uint layer)
    {
        var cellIdx = GetCell(position);
        if (cellIdx == -1) return;
        cells[cellIdx].Add(data, position, layer, radius);
        biggestRadius = float.Max(biggestRadius, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int GetCell(Vec2f position)
    {
        int x = (int)(position.X / cellSize);
        int y = (int)(position.Y / cellSize);
        if (x < 0 || x >= width || y < 0 || y >= height) return -1;
        return x + y * width;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<TData> results) where TLayer : unmanaged, Enum
    {
        return Query(position, radius, Unsafe.As<TLayer, uint>(ref layer), results);
    }

    public int Query(Vec2f position, float radius, uint layer, Span<TData> result)
    {
        var queryPos = position;
        var expandedRadius = radius + biggestRadius;
        var minX = (int)System.Math.Floor((queryPos.X - expandedRadius) / cellSize);
        var maxX = (int)System.Math.Ceiling((queryPos.X + expandedRadius) / cellSize);
        var minY = (int)System.Math.Floor((queryPos.Y - expandedRadius) / cellSize);
        var maxY = (int)System.Math.Ceiling((queryPos.Y + expandedRadius) / cellSize);
        minX = int.Clamp(minX, 0, width - 1);
        maxX = int.Clamp(maxX, 0, width - 1);
        minY = int.Clamp(minY, 0, height - 1);
        maxY = int.Clamp(maxY, 0, height - 1);

        var radiusSqr = radius * radius;
        var count = 0;
        var cellsSpan = cells.AsSpan();

        Span<QueryResult<TData>> results = stackalloc QueryResult<TData>[result.Length];

        for (int y = minY; y <= maxY; y++)
        {
            var row = cellsSpan.Slice(y * width + minX, maxX - minX + 1);
            for (int x = 0; x < row.Length; x++)
            {
                scoped ref var cell = ref row[x];
                if (cell.Count == 0 || !cell.HasLayer(layer)) continue;

                scoped ref var curr = ref cell.Entries[0];
                for (int i = 0; i < cell.Count; i++, curr = ref Unsafe.Add(ref curr, 1))
                {
                    if (!curr.HasLayer(layer)) continue;
                    var currRadiusSqr = curr.Radius * curr.Radius;
                    var distanceSqr = Vec2f.DistanceSquared(curr.Position, queryPos);
                    var sumRadiusSqr = (radius + curr.Radius) * (radius + curr.Radius);
                    if (distanceSqr >= sumRadiusSqr) continue;
                    SpatialQueryUtils.TryInsert(results, ref count, curr.Data, distanceSqr);
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            result[i] = results[i].Data;
        }

        return count;
    }
}