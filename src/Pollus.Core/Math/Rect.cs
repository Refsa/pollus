namespace Pollus.Mathematics;

public struct Rect
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

    public bool Overlaps(Rect other)
    {
        return!(Max.X < other.Min.X || Min.X > other.Max.X || Max.Y < other.Min.Y || Min.Y > other.Max.Y);
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
}