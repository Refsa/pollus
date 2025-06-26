namespace Pollus.Mathematics;

using System.Diagnostics;
using Pollus.Graphics;

[ShaderType]
[DebuggerDisplay("Rect: {Min} {Max}")]
public partial struct RectInt
{
    public Vec2<int> Min;
    public Vec2<int> Max;

    public int Width => Max.X - Min.X;
    public int Height => Max.Y - Min.Y;

    public RectInt(Vec2<int> min, Vec2<int> max)
    {
        Min = min;
        Max = max;
    }

    public RectInt(int minX, int minY, int maxX, int maxY)
    {
        Min = new Vec2<int>(minX, minY);
        Max = new Vec2<int>(maxX, maxY);
    }

    public static RectInt FromCenterScale(Vec2<int> center, Vec2<int> scale)
    {
        return new RectInt(center - scale / 2, center + scale / 2);
    }

    public static RectInt FromOriginSize(Vec2<int> origin, Vec2<int> size)
    {
        return new RectInt(origin, origin + size);
    }

    public static implicit operator Vec4<int>(in RectInt rect)
    {
        return new Vec4<int>(rect.Min.X, rect.Min.Y, rect.Max.X, rect.Max.Y);
    }

    public void Scale(Vec2<int> scale)
    {
        Min *= scale;
        Max *= scale;
    }

    public void ScaleCentered(Vec2<int> scale)
    {
        var center = Center();
        Min = center - (Max - Min) * scale / 2;
        Max = center + (Max - Min) * scale / 2;
    }

    public void Move(Vec2<int> offset)
    {
        Min += offset;
        Max += offset;
    }

    public Vec2<int> Center()
    {
        return (Min + Max) / 2;
    }

    public Vec2<int> TopLeft()
    {
        return Min;
    }

    public Vec2<int> TopRight()
    {
        return new Vec2<int>(Max.X, Min.Y);
    }

    public Vec2<int> BottomLeft()
    {
        return new Vec2<int>(Min.X, Max.Y);
    }

    public Vec2<int> BottomRight()
    {
        return Max;
    }

    public Vec2<int> Size()
    {
        return Max - Min;
    }

    public Vec2<int> Extents()
    {
        return (Max - Min) / 2;
    }

    public bool Contains(Vec2<int> point)
    {
        return Min.X <= point.X && Max.X >= point.X && Min.Y <= point.Y && Max.Y >= point.Y;
    }

    public bool InsideOf(Rect other)
    {
        return Min.X >= other.Min.X && Max.X <= other.Max.X && Min.Y >= other.Min.Y && Max.Y <= other.Max.Y;
    }

    public bool Contains(Rect other)
    {
        return Min.X <= other.Min.X && Max.X >= other.Max.X && Min.Y <= other.Min.Y && Max.Y >= other.Max.Y;
    }

    public bool Intersects(Rect other)
    {
        return Min.X < other.Max.X && Max.X > other.Min.X && Min.Y < other.Max.Y && Max.Y > other.Min.Y;
    }

    public Vec2<int> IntersectionPoint(RectInt other)
    {
        var x1 = Math.Max(Min.X, other.Min.X);
        var x2 = Math.Min(Max.X, other.Max.X);
        var y1 = Math.Max(Min.Y, other.Min.Y);
        var y2 = Math.Min(Max.Y, other.Max.Y);

        if (x1 < x2 && y1 < y2)
        {
            return new Vec2<int>(x1, y1);
        }
        return Vec2<int>.Zero;
    }

    public Vec2<int> IntersectionNormal(Rect other)
    {
        var x1 = Math.Max(Min.X, other.Min.X);
        var x2 = Math.Min(Max.X, other.Max.X);
        var y1 = Math.Max(Min.Y, other.Min.Y);
        var y2 = Math.Min(Max.Y, other.Max.Y);

        if (x1 >= x2 || y1 >= y2)
        {
            return Vec2<int>.Zero; // No intersection
        }

        var intersectionCenterX = (x1 + x2) / 2;
        var intersectionCenterY = (y1 + y2) / 2;

        var rectCenterX = (Min.X + Max.X) / 2;
        var rectCenterY = (Min.Y + Max.Y) / 2;

        var dx = intersectionCenterX - rectCenterX;
        var dy = intersectionCenterY - rectCenterY;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            return dx < 0 ? Vec2<int>.Left : Vec2<int>.Right; // Left or Right side
        }
        else
        {
            return dy < 0 ? Vec2<int>.Down : Vec2<int>.Up; // Bottom or Top side
        }
    }

    public override string ToString()
    {
        return $"Rect {{Min: {Min}, Max: {Max}}}";
    }
}