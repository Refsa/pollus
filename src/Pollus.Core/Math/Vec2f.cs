namespace Pollus.Mathematics;

using Pollus.Utils;
using Pollus.Core.Serialization;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Pollus.Graphics;

[ShaderType, Serialize, Reflect]
[DebuggerDisplay("Vec2f: {X}, {Y}")]
public partial record struct Vec2f
{
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Vec2f(in System.Numerics.Vector2 vector2)
    {
        return new Vec2f(vector2.X, vector2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator System.Numerics.Vector2(in Vec2f vector2)
    {
        return new System.Numerics.Vector2(vector2.X, vector2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Vec2f(in (float, float) tuple)
    {
        return new Vec2f(tuple.Item1, tuple.Item2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator +(in Vec2f left, in Vec2f right)
    {
        return new Vec2f(left.X + right.X, left.Y + right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator -(in Vec2f left, in Vec2f right)
    {
        return new Vec2f(left.X - right.X, left.Y - right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator -(in Vec2f left)
    {
        return new Vec2f(-left.X, -left.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator *(in Vec2f left, float right)
    {
        return new Vec2f(left.X * right, left.Y * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator *(float left, in Vec2f right)
    {
        return new Vec2f(left * right.X, left * right.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator *(in Vec2f left, in Vec2f right)
    {
        return new Vec2f(left.X * right.X, left.Y * right.Y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f operator /(in Vec2f left, float right)
    {
        return new Vec2f(left.X / right, left.Y / right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Angle()
    {
        return float.Atan2(Y, X);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Length()
    {
        return float.Sqrt(X * X + Y * Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float LengthSquared()
    {
        return X * X + Y * Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Normalized()
    {
        float length = Length();
        if (length == 0) return Zero;
        return new Vec2f(X / length, Y / length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Clamp(in Vec2f min, in Vec2f max)
    {
        return new Vec2f(float.Clamp(X, min.X, max.X), float.Clamp(Y, min.Y, max.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f ClampLength(float min, float max)
    {
        float length = Length();
        if (length == 0) return Zero;
        return Normalized() * float.Clamp(length, min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Dot(in Vec2f other)
    {
        return X * other.X + Y * other.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Cross(in Vec2f other)
    {
        return new Vec2f(X * other.Y - Y * other.X, Y * other.X - X * other.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Reflect(in Vec2f normal)
    {
        return this - 2f * Dot(normal) * normal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Rotate(float angle)
    {
        float cos = float.Cos(angle);
        float sin = float.Sin(angle);
        return new Vec2f(X * cos - Y * sin, X * sin + Y * cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vec2f Rotate(float angle, in Vec2f center)
    {
        Vec2f offset = this - center;
        return center + offset.Rotate(angle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly bool Approximately(in Vec2f other, float tolerance = float.Epsilon)
    {
        return X.Approximately(other.X, tolerance) && Y.Approximately(other.Y, tolerance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly Vec2f Abs()
    {
        return new Vec2f(float.Abs(X), float.Abs(Y));
    }

    public readonly Vec3f XYZ(float z = 0f)
    {
        return new Vec3f(X, Y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f Min(in Vec2f a, in Vec2f b)
    {
        return new Vec2f(float.Min(a.X, b.X), float.Min(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f Max(in Vec2f a, in Vec2f b)
    {
        return new Vec2f(float.Max(a.X, b.X), float.Max(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vec2f Lerp(in Vec2f a, in Vec2f b, float t)
    {
        return new Vec2f(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Distance(in Vec2f a, in Vec2f b)
    {
        return float.Sqrt(DistanceSquared(a, b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float DistanceSquared(in Vec2f a, in Vec2f b)
    {
        return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }
}
