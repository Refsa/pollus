namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;

public record struct Vec4f
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

    public static Vec4f operator *(Vec4f left, Vec4f right)
    {
        return new Vec4f(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }
}