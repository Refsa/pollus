namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;
using Pollus.Graphics;

[ShaderType]
public partial record struct Vec4f
{
    public static int SizeInBytes => Unsafe.SizeOf<float>() * 4;

    public static Vec4f Zero => new(0f, 0f, 0f, 0f);
    public static Vec4f UnitX => new(1f, 0f, 0f, 0f);
    public static Vec4f UnitY => new(0f, 1f, 0f, 0f);
    public static Vec4f UnitZ => new(0f, 0f, 1f, 0f);
    public static Vec4f UnitW => new(0f, 0f, 0f, 1f);

    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vec4f(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4f(Vec3f vec3, float w)
    {
        X = vec3.X;
        Y = vec3.Y;
        Z = vec3.Z;
        W = w;
    }

    public Vec3f Truncate()
    {
        return new Vec3f(X, Y, Z);
    }

    public Vec4f Dot(Vec4f other)
    {
        return new Vec4f(X * other.X, Y * other.Y, Z * other.Z, W * other.W);
    }

    public Vec4f Cross(Vec4f other)
    {
        return new Vec4f(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X,
            0f
        );
    }

    public float Length()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
    }

    public Vec4f Normalize()
    {
        var length = Length();
        return new Vec4f(X / length, Y / length, Z / length, W / length);
    }

    public static Vec4f operator +(Vec4f left, Vec4f right)
    {
        return new Vec4f(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vec4f operator -(Vec4f left, Vec4f right)
    {
        return new Vec4f(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vec4f operator *(Vec4f left, float right)
    {
        return new Vec4f(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    public static Vec4f operator *(float left, Vec4f right)
    {
        return new Vec4f(left * right.X, left * right.Y, left * right.Z, left * right.W);
    }

    public static Vec4f operator *(Vec4f left, Vec4f right)
    {
        return new Vec4f(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }
}