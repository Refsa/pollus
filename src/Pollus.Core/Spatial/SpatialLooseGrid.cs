namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Mathematics;

/// <summary>
/// Loose/Tight double grid
/// 
/// Works well for colliders with vastly different bounds
/// 
/// https://stackoverflow.com/a/48384354
/// https://github.com/loosegrid/DragonSpace-Demo/blob/master/Assets/Scripts/DragonSpace/Grids/LooseDoubleGrid.cs
/// </summary>
public class SpatialLooseGrid<TData> : ISpatialContainer<TData>
    where TData : unmanaged
{
    struct LooseCell
    {
        public int TopRightX;
        public int BottomLeftX;
        public int TopRightY;
        public int BottomLeftY;

        Content[] contents;
        int count;

        public Span<Content> Contents => contents.AsSpan(0, count);
        public int Count => count;

        public LooseCell()
        {
            contents = new Content[1];
        }

        public void Clear()
        {
            count = 0;
        }

        public void Insert(in Content content)
        {
            if (count == contents.Length) Array.Resize(ref contents, contents.Length * 2);
            contents[count++] = content;
        }
    }

    struct LooseRow
    {
        public LooseCell[] Cells { get; }

        public LooseRow(int count)
        {
            Cells = new LooseCell[count];
            Array.Fill(Cells, new LooseCell());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < Cells.Length; i++) Cells[i].Clear();
        }
    }

    struct TightCell
    {
        int[] elements;
        public int Count;
        public uint LayerMask;

        public Span<int> Elements => elements.AsSpan(0, Count);

        public TightCell(int count)
        {
            elements = new int[count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
            LayerMask = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int element, uint layer)
        {
            if (Count == elements.Length) Array.Resize(ref elements, elements.Length * 2);
            elements[Count++] = element;
            LayerMask |= layer;
        }
    }

    struct TightGrid
    {
        public TightCell[] Cells { get; }

        public TightGrid(int width, int height)
        {
            Cells = new TightCell[width * height];
            for (int i = 0; i < Cells.Length; i++) Cells[i] = new TightCell(1);
        }
    }

    struct Content
    {
        public TData Data;
        public Vec2f Position;
        public float Radius;
        public uint Layer;
    }

    int worldSize;
    int cellSize;
    int tightSize;

    int rowCount; int colCount;
    int tightRowCount; int tightColCount;
    float invCellWidth; float invCellHeight;
    float invTightWidth; float invTightHeight;

    LooseRow[] rows;
    TightGrid grid;

    public SpatialLooseGrid(int cellSize, int tightSize, int worldSize)
    {
        this.worldSize = worldSize;
        this.cellSize = cellSize;
        this.tightSize = tightSize;

        rowCount = worldSize / cellSize + 1;
        colCount = worldSize / cellSize + 1;
        tightRowCount = worldSize / tightSize + 1;
        tightColCount = worldSize / tightSize + 1;

        invCellWidth = 1f / cellSize; invCellHeight = 1f / cellSize;
        invTightWidth = 1f / tightSize; invTightHeight = 1f / tightSize;

        rows = new LooseRow[rowCount];
        for (int i = 0; i < rows.Length; i++) rows[i] = new LooseRow(colCount);
        grid = new TightGrid(tightColCount, tightRowCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        for (int i = 0; i < rows.Length; i++) rows[i].Clear();
        for (int y = 0; y < tightRowCount; y++)
        {
            for (int x = 0; x < tightColCount; x++)
            {
                grid.Cells[x + y * tightColCount].Clear();
            }
        }
    }

    public void Prepare()
    {
        // TightenUp();
        Clear();
    }

    public void TightenUp()
    {
        for (int y = 0; y < tightRowCount; y++)
        {
            for (int x = 0; x < tightColCount; x++)
            {
                grid.Cells[x + y * tightColCount].Clear();
            }
        }

        for (int i = rows.Length - 1; i >= 0; i--)
        {
            for (int j = rows[i].Cells.Length - 1; j >= 0; j--)
            {
                ref var cell = ref rows[i].Cells[j];
                CompactCell(ref cell);
                InsertToGrid(ref cell, i, j, 0);
            }
        }
    }

    public int Query(Vec2f position, float radius, uint layer, Span<TData> result)
    {
        var minX = PosToTightCol(position.X - radius);
        var maxX = PosToTightCol(position.X + radius);
        var minY = PosToTightRow(position.Y - radius);
        var maxY = PosToTightRow(position.Y + radius);

        var radiusSqr = radius * radius;
        var count = 0;

        Span<QueryResult<TData>> results = stackalloc QueryResult<TData>[result.Length];

        for (int y = minY; y <= maxY; y++)
        {
            var tightCells = grid.Cells.AsSpan(
                y * tightColCount + minX,
                maxX - minX + 1);

            for (int x = 0; x < tightCells.Length; x++)
            {
                var tightCellContents = tightCells[x].Elements;
                if (tightCellContents.Length == 0) continue;

                for (int i = 0; i < tightCellContents.Length; i++)
                {
                    int looseIndex = tightCellContents[i];
                    var contents = rows[looseIndex / colCount].Cells[looseIndex % colCount].Contents;

                    foreach (scoped ref var content in contents)
                    {
                        if ((content.Layer & layer) == 0) continue;
                        var contentRadiusSqr = content.Radius * content.Radius;
                        var distanceSqr = Vec2f.DistanceSquared(content.Position, position);
                        var sumRadius = radius + content.Radius;
                        if (distanceSqr >= sumRadius * sumRadius) continue;
                        SpatialQueryUtils.TryInsert(results, ref count, content.Data, distanceSqr);
                    }
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            result[i] = results[i].Data;
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<TData> results) where TLayer : unmanaged, Enum
    {
        return Query(position, radius, Unsafe.As<TLayer, uint>(ref layer), results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert<TLayer>(TData data, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        Insert(data, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(TData data, Vec2f position, float radius, uint layer)
    {
        var content = new Content { Data = data, Layer = layer, Position = position, Radius = radius };
        int cellRow = PosToCellRow(position.Y - radius);
        int cellCol = PosToCellCol(position.X - radius);

        ref var cell = ref rows[cellRow].Cells[cellCol];

        if (cell.Count == 0)
        {
            cell.Insert(content);
            cell.BottomLeftX = (int)(content.Position.X - content.Radius);
            cell.BottomLeftY = (int)(content.Position.Y - content.Radius);
            cell.TopRightX = (int)(content.Position.X + content.Radius);
            cell.TopRightY = (int)(content.Position.Y + content.Radius);

            InsertToGrid(ref cell, cellRow, cellCol, layer);
        }
        else
        {
            cell.Insert(content);
            ExpandCell(ref cell, content, cellRow, cellCol, layer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InsertToGrid(ref LooseCell cell, int cellRow, int cellCol, uint layer)
    {
        var minX = PosToTightCol(cell.BottomLeftX);
        var maxX = PosToTightCol(cell.TopRightX);
        var minY = PosToTightRow(cell.BottomLeftY);
        var maxY = PosToTightRow(cell.TopRightY);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                grid.Cells[x + y * tightColCount].Insert(cellRow * colCount + cellCol, layer);
            }
        }
    }

    void ExpandCell(ref LooseCell cell, in Content content, int cellRow, int cellCol, uint layer)
    {
        var xMin = PosToTightCol(cell.BottomLeftX);
        var xMax = PosToTightCol(cell.TopRightX);
        var yMin = PosToTightRow(cell.BottomLeftY);
        var yMax = PosToTightRow(cell.TopRightY);

        cell.BottomLeftY = int.Min(cell.BottomLeftY, (int)(content.Position.Y - content.Radius));
        cell.BottomLeftX = int.Min(cell.BottomLeftX, (int)(content.Position.X - content.Radius));
        cell.TopRightY = int.Max(cell.TopRightY, (int)(content.Position.Y + content.Radius));
        cell.TopRightX = int.Max(cell.TopRightX, (int)(content.Position.X + content.Radius));

        var yMax2 = PosToTightRow(cell.TopRightY);
        var xMax2 = PosToTightCol(cell.TopRightX);

        var xDiff = (xMax2 > xMax) ? 1 : 0;
        if (xMax != xMax2 || yMax != yMax2)
        {
            for (int y = yMin + 1; y <= yMax2; y++)
            {
                int x = (y > yMax) ? xMin : xMax + xDiff;
                for (; x <= xMax2; x++)
                {
                    grid.Cells[x + y * tightColCount].Insert(cellRow * colCount + cellCol, layer);
                }
            }
        }
    }

    void CompactCell(ref LooseCell cell)
    {
        cell.BottomLeftY = cell.BottomLeftX = int.MaxValue;
        cell.TopRightY = cell.TopRightX = int.MinValue;
        if (cell.Count == 0) return;

        foreach (ref var content in cell.Contents)
        {
            cell.BottomLeftY = int.Min(cell.BottomLeftY, (int)(content.Position.Y - content.Radius));
            cell.BottomLeftX = int.Min(cell.BottomLeftX, (int)(content.Position.X - content.Radius));
            cell.TopRightY = int.Max(cell.TopRightY, (int)(content.Position.Y + content.Radius));
            cell.TopRightX = int.Max(cell.TopRightX, (int)(content.Position.X + content.Radius));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PosToTightRow(float y)
    {
        if (y <= 0) return 0;
        return int.Min((int)(y * invTightHeight), tightRowCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PosToTightCol(float x)
    {
        if (x <= 0) return 0;
        return int.Min((int)(x * invTightWidth), tightColCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PosToCellRow(float y)
    {
        if (y <= 0) return 0;
        return int.Min((int)(y * invCellHeight), rowCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PosToCellCol(float x)
    {
        if (x <= 0) return 0;
        return int.Min((int)(x * invCellWidth), colCount - 1);
    }

    public IEnumerable<Rect> GetLooseBounds()
    {
        for (int y = 0; y < rowCount; y++)
        {
            for (int x = 0; x < colCount; x++)
            {
                var cell = rows[y].Cells[x];
                if (cell.Count == 0) continue;

                yield return new Rect(
                    cell.BottomLeftX, cell.BottomLeftY,
                    cell.TopRightX, cell.TopRightY
                );
            }
        }
    }

    public IEnumerable<Rect> GetTightBounds()
    {
        for (int y = 0; y < tightRowCount; y++)
        {
            for (int x = 0; x < tightColCount; x++)
            {
                var cell = grid.Cells[x + y * tightColCount];
                if (cell.Count == 0) continue;

                yield return new Rect(
                    x * tightSize, y * tightSize,
                    x * tightSize + tightSize, y * tightSize + tightSize
                );
            }
        }
    }
}