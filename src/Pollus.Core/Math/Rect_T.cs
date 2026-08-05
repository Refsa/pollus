namespace Pollus.Mathematics;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Pollus.Core.Serialization;
using Pollus.Graphics;
using Pollus.Utils;

[DebuggerDisplay("Rect: {X}, {Y}, {Width}, {Height}")]
[ShaderType, Reflect, Serialize]
public partial struct Rect<T> : IEquatable<Rect<T>>, IEqualityComparer<Rect<T>>
    where T : unmanaged, INumber<T>
{
    public static readonly Rect<T> Zero = new(T.Zero, T.Zero, T.Zero, T.Zero);

    public T X;
    public T Y;
    public T Width;
    public T Height;

    public readonly Vec2<T> Min => new(X, Y);
    public readonly Vec2<T> Max => new(X + Width, Y + Height);

    public Rect(Vec2<T> origin, Vec2<T> size)
    {
        X = origin.X;
        Y = origin.Y;
        Width = size.X;
        Height = size.Y;
    }

    public Rect(T x, T y, T width, T height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static Rect<T> FromCenterScale(Vec2<T> center, Vec2<T> scale)
    {
        return new Rect<T>(center - scale / T.CreateChecked(2), scale);
    }

    public static Rect<T> FromOriginSize(Vec2<T> origin, Vec2<T> size)
    {
        return new Rect<T>(origin, size);
    }

    public static Rect<T> FromMinMax(Vec2<T> min, Vec2<T> max)
    {
        return new Rect<T>(min, max - min);
    }

    public static implicit operator Vec4<T>(in Rect<T> rect)
    {
        return new Vec4<T>(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static bool operator ==(Rect<T> left, Rect<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect<T> left, Rect<T> right)
    {
        return !left.Equals(right);
    }

    public void Scale(Vec2<T> scale)
    {
        X *= scale.X;
        Y *= scale.Y;
        Width *= scale.X;
        Height *= scale.Y;
    }

    public void ScaleCentered(Vec2<T> scale)
    {
        var center = Center();
        Width *= scale.X;
        Height *= scale.Y;
        X = center.X - Width / T.CreateChecked(2);
        Y = center.Y - Height / T.CreateChecked(2);
    }

    public void Move(Vec2<T> offset)
    {
        X += offset.X;
        Y += offset.Y;
    }

    public void Expand(Vec2<T> point)
    {
        var minX = T.Min(X, point.X);
        var minY = T.Min(Y, point.Y);
        var maxX = T.Max(X + Width, point.X);
        var maxY = T.Max(Y + Height, point.Y);

        X = minX;
        Y = minY;
        Width = maxX - minX;
        Height = maxY - minY;
    }

    public void Expand(params ReadOnlySpan<Vec2<T>> points)
    {
        foreach (var point in points)
        {
            Expand(point);
        }
    }

    public readonly Vec2<T> Center()
    {
        return new Vec2<T>(
            X + Width / T.CreateChecked(2),
            Y + Height / T.CreateChecked(2)
        );
    }

    public readonly Vec2<T> TopLeft()
    {
        return new Vec2<T>(X, Y);
    }

    public readonly Vec2<T> TopRight()
    {
        return new Vec2<T>(X + Width, Y);
    }

    public readonly Vec2<T> BottomLeft()
    {
        return new Vec2<T>(X, Y + Height);
    }

    public readonly Vec2<T> BottomRight()
    {
        return new Vec2<T>(X + Width, Y + Height);
    }

    public readonly Vec2<T> Size()
    {
        return new Vec2<T>(Width, Height);
    }

    public readonly Vec2<T> Extents()
    {
        return new Vec2<T>(
            Width / T.CreateChecked(2),
            Height / T.CreateChecked(2)
        );
    }

    public readonly bool Contains(Vec2<T> point)
    {
        return X <= point.X && X + Width >= point.X &&
            Y <= point.Y && Y + Height >= point.Y;
    }

    public readonly bool InsideOf(Rect<T> other)
    {
        return X >= other.X && X + Width <= other.X + other.Width &&
            Y >= other.Y && Y + Height <= other.Y + other.Height;
    }

    public readonly bool Contains(Rect<T> other)
    {
        return X <= other.X && X + Width >= other.X + other.Width &&
            Y <= other.Y && Y + Height >= other.Y + other.Height;
    }

    public readonly bool Intersects(Rect<T> other)
    {
        return X < other.X + other.Width && X + Width > other.X &&
            Y < other.Y + other.Height && Y + Height > other.Y;
    }

    public readonly Vec2<T> IntersectionPoint(Rect<T> other)
    {
        var x1 = T.Max(X, other.X);
        var x2 = T.Min(X + Width, other.X + other.Width);
        var y1 = T.Max(Y, other.Y);
        var y2 = T.Min(Y + Height, other.Y + other.Height);

        return x1 < x2 && y1 < y2
            ? new Vec2<T>(x1, y1)
            : Vec2<T>.Zero;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Rect<T> other && Equals(other);
    }

    public readonly bool Equals(Rect<T> other)
    {
        return X == other.X && Y == other.Y &&
            Width == other.Width && Height == other.Height;
    }

    public readonly bool Equals(Rect<T> x, Rect<T> y)
    {
        return x == y;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public readonly int GetHashCode(Rect<T> obj)
    {
        return obj.GetHashCode();
    }

    public override readonly string ToString()
    {
        return $"Rect {{X: {X}, Y: {Y}, Width: {Width}, Height: {Height}}}";
    }
}


public static class RectTExt
{
    public static bool NullableEquals<T>(this in Rect<T>? a, in Rect<T>? b)
        where T : unmanaged, INumber<T>
    {
        if (a.HasValue != b.HasValue) return false;
        if (!a.HasValue) return true;
        return a!.Value.Min == b!.Value.Min && a.Value.Max == b.Value.Max;
    }
}