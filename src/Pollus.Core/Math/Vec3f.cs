namespace Pollus.Mathematics;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pollus.Graphics;
using Pollus.Core.Serialization;
using Pollus.Utils;

[ShaderType, Reflect, Serialize]
[DebuggerDisplay("Vec3f: {X}, {Y}, {Z}")]
public partial record struct Vec3f
{
    public static Vec3f Zero => new(0f, 0f, 0f);
    public static Vec3f One => new(1f, 1f, 1f);
    public static Vec3f Forward => new(0f, 0f, 1f);
    public static Vec3f Backward => new(0f, 0f, -1f);
    public static Vec3f Up => new(0f, 1f, 0f);
    public static Vec3f Down => new(0f, -1f, 0f);
    public static Vec3f Right => new(1f, 0f, 0f);
    public static Vec3f Left => new(-1f, 0f, 0f);

    public float X;
    public float Y;
    public float Z;

    public Vec3f(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3f(Vec2f other, float z)
    {
        X = other.X;
        Y = other.Y;
        Z = z;
    }

    public readonly Vec2f XY => new(X, Y);
    public readonly Vec2f XZ => new(X, Z);
    public readonly Vec2f YZ => new(Y, Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f(in (float x, float y, float z) tuple)
    {
        return new Vec3f(tuple.x, tuple.y, tuple.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vec3f(in System.Numerics.Vector3 vector3)
    {
        return new Vec3f(vector3.X, vector3.Y, vector3.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector3(in Vec3f vector3)
    {
        return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator +(in Vec3f left, in Vec3f right)
    {
        return new Vec3f(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator -(in Vec3f left, in Vec3f right)
    {
        return new Vec3f(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator -(in Vec3f vec)
    {
        return new Vec3f(-vec.X, -vec.Y, -vec.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(in Vec3f left, float right)
    {
        return new Vec3f(left.X * right, left.Y * right, left.Z * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(float right, in Vec3f left)
    {
        return new Vec3f(left.X * right, left.Y * right, left.Z * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3f operator *(in Vec3f left, in Vec3f right)
    {
        return new Vec3f(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length()
    {
        return float.Sqrt(X * X + Y * Y + Z * Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f Normalized()
    {
        float length = Length();
        return new Vec3f(X / length, Y / length, Z / length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Dot(Vec3f other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3f Cross(Vec3f other)
    {
        return new Vec3f(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }
}
