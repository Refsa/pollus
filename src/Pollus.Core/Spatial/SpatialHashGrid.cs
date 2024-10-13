namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Mathematics;

public class SpatialHashGrid<TData> : ISpatialContainer<TData>
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
    readonly Vec2f offset;

    Cell[] cells;

    public SpatialHashGrid(int cellSize, int width, int height)
    {
        this.cellSize = cellSize;
        this.width = width;
        this.height = height;
        offset = new Vec2f(width / 2, height / 2);

        cells = new Cell[width * height];
        for (int i = 0; i < cells.Length; i++) cells[i] = new Cell();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        foreach (ref var cell in cells.AsSpan()) cell.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(TData entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        Insert(entity, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(TData data, Vec2f position, float radius, uint layer)
    {
        var cellIdx = GetCell(position);
        if (cellIdx == -1) return;
        cells[cellIdx].Add(data, position, layer, radius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int GetCell(Vec2f position)
    {
        int x = (int)((position.X + offset.X) / cellSize);
        int y = (int)((position.Y + offset.Y) / cellSize);
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
        var offsetPosition = position + offset;
        var minX = (int)((offsetPosition.X - radius) / cellSize);
        var maxX = (int)((offsetPosition.X + radius) / cellSize);
        var minY = (int)((offsetPosition.Y - radius) / cellSize);
        var maxY = (int)((offsetPosition.Y + radius) / cellSize);
        minX = Math.Clamp(minX, 0, width - 1);
        maxX = Math.Clamp(maxX, 0, width - 1);
        minY = Math.Clamp(minY, 0, height - 1);
        maxY = Math.Clamp(maxY, 0, height - 1);

        var radiusSquared = radius * radius;
        var resultCursor = 0;
        var cellsSpan = cells.AsSpan();

        for (int y = minY; y <= maxY; y++)
        {
            var row = cellsSpan.Slice(y * width + minX, maxX - minX + 1);
            for (int x = 0; x < row.Length; x++)
            {
                scoped ref var cell = ref row[x];
                if (cell.Count == 0 || !cell.HasLayer(layer)) continue;

                ref var curr = ref cell.Entries[0];
                for (int i = 0; i < cell.Count; i++, curr = ref Unsafe.Add(ref curr, 1))
                {
                    if (!curr.HasLayer(layer)) continue;
                    if (Vec2f.DistanceSquared(curr.Position, position) > radiusSquared) continue;
                    result[resultCursor++] = curr.Data;
                    if (resultCursor >= result.Length) return resultCursor;
                }
            }
        }

        return resultCursor;
    }
}