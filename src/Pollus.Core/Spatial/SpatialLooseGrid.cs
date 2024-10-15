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
/// </summary>
public class SpatialLooseGrid<TData> : ISpatialContainer<TData>
{
    struct LooseCell
    {
        public int TopLeft;
        public int TopRight;
        public int BottomLeft;
        public int BottomRight;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            for (int i = 0; i < Cells.Length; i++) Cells[i].Clear();
        }
    }

    struct TightCell
    {
        public int[] Elements;
        public int Count;
        public uint LayerMask;

        public TightCell(int count)
        {
            Elements = new int[count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            Count = 0;
            LayerMask = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Insert(int element, uint layer)
        {
            if (Count == Elements.Length) Array.Resize(ref Elements, Elements.Length * 2);
            Elements[Count++] = element;
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

    int width; int height;
    int rowCount; int colCount;
    int tightRowCount; int tightColCount;
    float invCellWidth; float invCellHeight;
    float invTightWidth; float invTightHeight;

    LooseRow[] rows;
    TightGrid grid;

    public SpatialLooseGrid(int cellSize, int tightSize, int looseSize)
    {
        width = looseSize; height = looseSize;
        rowCount = height / cellSize + 1;
        colCount = width / cellSize + 1;
        tightRowCount = height / tightSize + 1;
        tightColCount = width / tightSize + 1;

        invCellWidth = 1f / cellSize; invCellHeight = 1f / cellSize;
        invTightWidth = 1f / tightSize; invTightHeight = 1f / tightSize;

        rows = new LooseRow[rowCount];
        for (int i = 0; i < rows.Length; i++) rows[i] = new LooseRow(colCount);
        grid = new TightGrid(tightColCount, tightRowCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        for (int y = minY; y <= maxY; y++)
        {
            var tightCells = grid.Cells.AsSpan(
                y * tightColCount + minX,
                maxX - minX + 1);

            for (int x = 0; x < tightCells.Length; x++)
            {
                scoped ref var tightCell = ref tightCells[x];
                if (tightCell.Count == 0 || (tightCell.LayerMask & layer) == 0) continue;

                for (int i = 0; i < tightCell.Count; i++)
                {
                    int looseIndex = tightCell.Elements[i];
                    var contents = rows[looseIndex / colCount].Cells[looseIndex % colCount].Contents;

                    foreach (scoped ref var content in contents)
                    {
                        if ((content.Layer & layer) == 0) continue;
                        var contentRadiusSqr = content.Radius * content.Radius;
                        if (Vec2f.DistanceSquared(content.Position, position) > radiusSqr + contentRadiusSqr) continue;

                        result[count++] = content.Data;
                        if (count >= result.Length) break;
                    }
                }
            }
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<TData> results) where TLayer : unmanaged, Enum
    {
        return Query(position, radius, Unsafe.As<TLayer, uint>(ref layer), results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(TData data, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        Insert(data, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(TData data, Vec2f position, float radius, uint layer)
    {
        var content = new Content { Data = data, Layer = layer, Position = position, Radius = radius };
        int cellRow = PosToCellRow(position.Y - radius);
        int cellCol = PosToCellCol(position.X - radius);
        InsertToLoose(content, cellRow, cellCol, layer);
    }

    void InsertToLoose(in Content content, int cellRow, int cellCol, uint layer)
    {
        ref var cell = ref rows[cellRow].Cells[cellCol];

        if (cell.Count == 0)
        {
            cell.Insert(content);
            cell.BottomLeft = (int)(content.Position.Y - content.Radius);
            cell.TopLeft = (int)(content.Position.X - content.Radius);
            cell.BottomRight = (int)(content.Position.Y + content.Radius);
            cell.TopRight = (int)(content.Position.X + content.Radius);

            InsertToGrid(ref cell, cellRow, cellCol, layer);
        }
        else
        {
            cell.Insert(content);
            ExpandCell(ref cell, content, cellRow, cellCol, layer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void InsertToGrid(ref LooseCell cell, int cellRow, int cellCol, uint layer)
    {
        var minX = PosToTightCol(cell.TopLeft);
        var maxX = PosToTightCol(cell.BottomRight);
        var minY = PosToTightRow(cell.BottomLeft);
        var maxY = PosToTightRow(cell.TopRight);

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
        var xMin = PosToTightCol(cell.TopLeft);
        var xMax = PosToTightCol(cell.BottomRight);
        var yMin = PosToTightRow(cell.BottomLeft);
        var yMax = PosToTightRow(cell.TopRight);

        cell.BottomLeft = int.Min(cell.BottomLeft, (int)(content.Position.Y - content.Radius));
        cell.TopLeft = int.Min(cell.TopLeft, (int)(content.Position.X - content.Radius));
        cell.BottomRight = int.Max(cell.BottomRight, (int)(content.Position.Y + content.Radius));
        cell.TopRight = int.Max(cell.TopRight, (int)(content.Position.X + content.Radius));

        var yMax2 = PosToTightRow(cell.TopRight);
        var xMax2 = PosToTightCol(cell.BottomRight);

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
        cell.BottomLeft = cell.TopLeft = int.MaxValue;
        cell.BottomRight = cell.TopRight = int.MinValue;
        if (cell.Count == 0) return;

        foreach (ref var content in cell.Contents)
        {
            cell.BottomLeft = int.Min(cell.BottomLeft, (int)(content.Position.Y - content.Radius));
            cell.TopLeft = int.Min(cell.TopLeft, (int)(content.Position.X - content.Radius));
            cell.BottomRight = int.Max(cell.BottomRight, (int)(content.Position.Y + content.Radius));
            cell.TopRight = int.Max(cell.TopRight, (int)(content.Position.X + content.Radius));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int PosToTightRow(float y)
    {
        if (y <= 0) return 0;
        return int.Min((int)(y * invTightHeight), tightRowCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int PosToTightCol(float x)
    {
        if (x <= 0) return 0;
        return int.Min((int)(x * invTightWidth), tightColCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int PosToCellRow(float y)
    {
        if (y <= 0) return 0;
        return int.Min((int)(y * invCellHeight), rowCount - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int PosToCellCol(float x)
    {
        if (x <= 0) return 0;
        return int.Min((int)(x * invCellWidth), colCount - 1);
    }
}