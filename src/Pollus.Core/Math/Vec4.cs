namespace Pollus.Mathematics;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pollus.Graphics;
using Pollus.Core.Serialization;
using Pollus.Utils;

[ShaderType, Reflect, Serialize]
[DebuggerDisplay("Vec4: {X}, {Y}, {Z}, {W}")]
public partial record struct Vec4<T>
    where T : struct, System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 4;

    public static Vec4<T> One => new Vec4<T>(T.One, T.One, T.One, T.One);
    public static Vec4<T> Zero => new Vec4<T>(default, default, default, default);
    public static Vec4<T> UnitX => new Vec4<T>(T.One, T.Zero, T.Zero, T.Zero);
    public static Vec4<T> UnitY => new Vec4<T>(T.Zero, T.One, T.Zero, T.Zero);
    public static Vec4<T> UnitZ => new Vec4<T>(T.Zero, T.Zero, T.One, T.Zero);
    public static Vec4<T> UnitW => new Vec4<T>(T.Zero, T.Zero, T.Zero, T.One);

    public T X;
    public T Y;
    public T Z;
    public T W;

    public Vec4(T x, T y, T z, T w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4(Vec3<T> vec3, T w)
    {
        X = vec3.X;
        Y = vec3.Y;
        Z = vec3.Z;
        W = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4<T> Mul(T scalar)
    {
        return new Vec4<T>(X * scalar, Y * scalar, Z * scalar, W * scalar);
    }

    public Vec3<T> Truncate()
    {
        return new Vec3<T>(X, Y, Z);
    }

    public static Vec4<T> operator +(Vec4<T> left, Vec4<T> right)
    {
        return new Vec4<T>(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vec4<T> operator -(Vec4<T> left, Vec4<T> right)
    {
        return new Vec4<T>(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vec4<T> operator *(Vec4<T> left, T right)
    {
        return new Vec4<T>(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    public static Vec4<T> operator *(T left, Vec4<T> right)
    {
        return new Vec4<T>(left * right.X, left * right.Y, left * right.Z, left * right.W);
    }

    public static Vec4<T> operator *(Vec4<T> left, Vec4<T> right)
    {
        return new Vec4<T>(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }
}