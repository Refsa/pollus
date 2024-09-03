namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;
using Pollus.Graphics;

[ShaderType]
public partial record struct Vec2f
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

    public static Vec2f operator -(Vec2f left)
    {
        return new Vec2f(-left.X, -left.Y);
    }

    public static Vec2f operator *(Vec2f left, float right)
    {
        return new Vec2f(left.X * right, left.Y * right);
    }

    public static Vec2f operator *(float left, Vec2f right)
    {
        return new Vec2f(left * right.X, left * right.Y);
    }

    public static Vec2f operator *(Vec2f left, Vec2f right)
    {
        return new Vec2f(left.X * right.X, left.Y * right.Y);
    }


    public static Vec2f operator /(Vec2f left, float right)
    {
        return new Vec2f(left.X / right, left.Y / right);
    }

    public float Length()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    public float LengthSquared()
    {
        return X * X + Y * Y;
    }

    public Vec2f Normalized()
    {
        float length = Length();
        if (length == 0) return Zero;
        return new Vec2f(X / length, Y / length);
    }

    public Vec2f Clamp(Vec2f min, Vec2f max)
    {
        return new Vec2f(Math.Clamp(X, min.X, max.X), Math.Clamp(Y, min.Y, max.Y));
    }

    public float Dot(Vec2f other)
    {
        return X * other.X + Y * other.Y;
    }

    public Vec2f Cross(Vec2f other)
    {
        return new Vec2f(X * other.Y - Y * other.X, Y * other.X - X * other.Y);
    }

    public Vec2f Reflect(Vec2f normal)
    {
        return this - 2f * Dot(normal) * normal;
    }

    public Vec2f Rotate(float angle)
    {
        float cos = Math.Cos(angle);
        float sin = Math.Sin(angle);
        return new Vec2f(X * cos - Y * sin, X * sin + Y * cos);
    }

    public Vec2f Rotate(float angle, Vec2f center)
    {
        Vec2f offset = this - center;
        return center + offset.Rotate(angle);
    }

    public bool Approximately(Vec2f other, float tolerance = float.Epsilon)
    {
        return X.Approximately(other.X, tolerance) && Y.Approximately(other.Y, tolerance);
    }

    public Vec2f Abs()
    {
        return new Vec2f(Math.Abs(X), Math.Abs(Y));
    }

    public static Vec2f Min(Vec2f a, Vec2f b)
    {
        return new Vec2f(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    public static Vec2f Max(Vec2f a, Vec2f b)
    {
        return new Vec2f(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }
}
