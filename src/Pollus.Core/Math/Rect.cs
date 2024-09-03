namespace Pollus.Mathematics;

using Pollus.Graphics;

[ShaderType]
public partial struct Rect
{
    public Vec2f Min;
    public Vec2f Max;

    public Rect(Vec2f min, Vec2f max)
    {
        Min = min;
        Max = max;
    }

    public Rect(float minX, float minY, float maxX, float maxY)
    {
        Min = new Vec2f(minX, minY);
        Max = new Vec2f(maxX, maxY);
    }

    public static Rect FromCenterScale(Vec2f center, Vec2f scale)
    {
        return new Rect(center - scale / 2, center + scale / 2);
    }

    public static implicit operator Vec4f(in Rect rect)
    {
        return new Vec4f(rect.Min.X, rect.Min.Y, rect.Max.X, rect.Max.Y);
    }

    public void Scale(Vec2f scale)
    {
        Min *= scale;
        Max *= scale;
    }

    public Vec2f Center()
    {
        return (Min + Max) / 2;
    }

    public Vec2f TopLeft()
    {
        return Min;
    }

    public Vec2f TopRight()
    {
        return new Vec2f(Max.X, Min.Y);
    }

    public Vec2f BottomLeft()
    {
        return new Vec2f(Min.X, Max.Y);
    }

    public Vec2f BottomRight()
    {
        return Max;
    }

    public Vec2f Size()
    {
        return Max - Min;
    }

    public Rect Move(Vec2f offset)
    {
        return new Rect(Min + offset, Max + offset);
    }

    public bool Contains(Vec2f point)
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

    public Vec2f IntersectionPoint(Rect other)
    {
        var x1 = Math.Max(Min.X, other.Min.X);
        var x2 = Math.Min(Max.X, other.Max.X);
        var y1 = Math.Max(Min.Y, other.Min.Y);
        var y2 = Math.Min(Max.Y, other.Max.Y);

        if (x1 < x2 && y1 < y2)
        {
            return new Vec2f(x1, y1);
        }
        return Vec2f.Zero;
    }

    public Vec2f IntersectionNormal(Rect other)
    {
        var x1 = Math.Max(Min.X, other.Min.X);
        var x2 = Math.Min(Max.X, other.Max.X);
        var y1 = Math.Max(Min.Y, other.Min.Y);
        var y2 = Math.Min(Max.Y, other.Max.Y);

        if (x1 >= x2 || y1 >= y2)
        {
            return Vec2f.Zero; // No intersection
        }

        var intersectionCenterX = (x1 + x2) / 2;
        var intersectionCenterY = (y1 + y2) / 2;

        var rectCenterX = (Min.X + Max.X) / 2;
        var rectCenterY = (Min.Y + Max.Y) / 2;

        var dx = intersectionCenterX - rectCenterX;
        var dy = intersectionCenterY - rectCenterY;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            return dx < 0 ? Vec2f.Left : Vec2f.Right; // Left or Right side
        }
        else
        {
            return dy < 0 ? Vec2f.Down : Vec2f.Up; // Bottom or Top side
        }
    }
}