namespace Pollus.Spatial;

using Pollus.Collections;
using Pollus.Mathematics;

public class SpatialHashGrid<TData>
{
    public struct CellEntry
    {
        public TData Data;
        public uint Layer;
        public float Radius;
        public Vec2f Position;
    }

    public class Cell
    {
        CellEntry[] entries = new CellEntry[1];
        int count;

        public ReadOnlySpan<CellEntry> Entries => entries.AsSpan(0, count);

        public Cell() { }

        public void Clear() => count = 0;
        public ref CellEntry this[int index] => ref entries[index];
        public ref CellEntry Next()
        {
            if (count == entries.Length) Array.Resize(ref entries, count * 2);
            return ref entries[count++];
        }
    }

    readonly int cellSize;
    readonly int width;
    readonly int height;
    readonly Vec2f offset;
    
    int entryCount;
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

    public void Clear()
    {
        entryCount = 0;
        foreach (ref var cell in cells.AsSpan()) cell.Clear();
    }

    public void Insert(TData data, Vec2f position, float radius, uint layer)
    {
        var cellIdx = GetCell(position);
        ref var cellEntry = ref cells[cellIdx].Next();
        cellEntry.Data = data;
        cellEntry.Layer = layer;
        cellEntry.Radius = radius;
        cellEntry.Position = position;
        entryCount++;
    }

    int GetCell(Vec2f position)
    {
        Vec2f offsetPosition = position + offset;
        int x = Math.Clamp((int)(offsetPosition.X / cellSize), 0, width - 1);
        int y = Math.Clamp((int)(offsetPosition.Y / cellSize), 0, height - 1);
        return x + y * width;
    }

    public void Query(Vec2f position, float radius, uint layer, ArrayList<CellEntry> list)
    {
        var offsetPosition = position + offset;
        var minX = Math.Clamp((int)((offsetPosition.X - radius) / cellSize), 0, width - 1);
        var maxX = Math.Clamp((int)((offsetPosition.X + radius) / cellSize), 0, width - 1);
        var minY = Math.Clamp((int)((offsetPosition.Y - radius) / cellSize), 0, height - 1);
        var maxY = Math.Clamp((int)((offsetPosition.Y + radius) / cellSize), 0, height - 1);

        var radiusSquared = radius * radius;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                foreach (var cellEntry in cells[x + y * width].Entries)
                {
                    if ((cellEntry.Layer & layer) != 0)
                    {
                        Vec2f relativePosition = cellEntry.Position - position;
                        if (relativePosition.LengthSquared() <= radiusSquared)
                        {
                            list.Add(cellEntry);
                        }
                    }
                }
            }
        }
    }
}