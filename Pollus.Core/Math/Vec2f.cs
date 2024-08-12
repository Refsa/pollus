namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;

public record struct Vec2f
{
    public static int SizeInBytes => Unsafe.SizeOf<float>() * 2;

    public static Vec2f Zero => new(0f, 0f);
    public static Vec2f One => new(1f, 1f);
    public static Vec2f Up => new(0f, 1f);
    public static Vec2f Down => new(0f, -1f);
    public static Vec2f Left => new(-1f, 0f);
    public static Vec2f Right => new(1f, 0f);


    public float X;
    public float Y;

    public Vec2f(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vec2f(System.Numerics.Vector2 vector2)
    {
        return new Vec2f(vector2.X, vector2.Y);
    }

    public static implicit operator System.Numerics.Vector2(Vec2f vector2)
    {
        return new System.Numerics.Vector2((float)Convert.ChangeType(vector2.X, typeof(float)), (float)Convert.ChangeType(vector2.Y, typeof(float)));
    }

    public static implicit operator Vec2f((float, float) tuple)
    {
        return new Vec2f(tuple.Item1, tuple.Item2);
    }

    public static Vec2f operator +(Vec2f left, Vec2f right)
    {
        return new Vec2f(left.X + right.X, left.Y + right.Y);
    }

    public static Vec2f operator -(Vec2f left, Vec2f right)
    {
        return new Vec2f(left.X - right.X, left.Y - right.Y);
    }

    public static Vec2f operator *(Vec2f left, float right)
    {
        return new Vec2f(left.X * right, left.Y * right);
    }

    public float Length()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    public Vec2f Normalized()
    {
        float length = Length();
        if (length == 0) return Zero;
        return new Vec2f(X / length, Y / length);
    }
}
