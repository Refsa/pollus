namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;

public record struct Vec2<T>
    where T : struct, System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 2;

    public T X;
    public T Y;

    public Vec2(T x, T y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vec2<T>(System.Numerics.Vector2 vector2)
    {
        return new Vec2<T>((T)Convert.ChangeType(vector2.X, typeof(T)), (T)Convert.ChangeType(vector2.Y, typeof(T)));
    }

    public static implicit operator System.Numerics.Vector2(Vec2<T> vector2)
    {
        return new System.Numerics.Vector2((float)Convert.ChangeType(vector2.X, typeof(float)), (float)Convert.ChangeType(vector2.Y, typeof(float)));
    }

    public static implicit operator Vec2<T>((T, T) tuple)
    {
        return new Vec2<T>(tuple.Item1, tuple.Item2);
    }
}
