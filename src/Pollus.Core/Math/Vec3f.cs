namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;
using Pollus.Graphics;

[ShaderType]
public partial record struct Vec3f
{
    public static int SizeInBytes => Unsafe.SizeOf<float>() * 3;
    public static Vec3f Zero => new Vec3f(0f, 0f, 0f);
    public static Vec3f One => new Vec3f(1f, 1f, 1f);
    public static Vec3f Forward => new Vec3f(0f, 0f, 1f);
    public static Vec3f Backward => new Vec3f(0f, 0f, -1f);
    public static Vec3f Up => new Vec3f(0f, 1f, 0f);
    public static Vec3f Down => new Vec3f(0f, -1f, 0f);
    public static Vec3f Right => new Vec3f(1f, 0f, 0f);
    public static Vec3f Left => new Vec3f(-1f, 0f, 0f);

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

    public Vec2f XY => new Vec2f(X, Y);
    public Vec2f XZ => new Vec2f(X, Z);
    public Vec2f YZ => new Vec2f(Y, Z);

    public static implicit operator Vec3f(in (float x, float y, float z) tuple)
    {
        return new Vec3f(tuple.x, tuple.y, tuple.z);
    }

    public static implicit operator Vec3f(in System.Numerics.Vector3 vector3)
    {
        return new Vec3f(vector3.X, vector3.Y, vector3.Z);
    }

    public static implicit operator System.Numerics.Vector3(in Vec3f vector3)
    {
        return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
    }

    public static Vec3f operator +(in Vec3f left, in Vec3f right)
    {
        return new Vec3f(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vec3f operator -(in Vec3f left, in Vec3f right)
    {
        return new Vec3f(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vec3f operator -(in Vec3f vec)
    {
        return new Vec3f(-vec.X, -vec.Y, -vec.Z);
    }

    public static Vec3f operator *(in Vec3f left, float right)
    {
        return new Vec3f(left.X * right, left.Y * right, left.Z * right);
    }

    public static Vec3f operator *(float right, in Vec3f left)
    {
        return new Vec3f(left.X * right, left.Y * right, left.Z * right);
    }

    public float Length()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public Vec3f Normalized()
    {
        float length = Length();
        return new Vec3f(X / length, Y / length, Z / length);
    }

    public float Dot(Vec3f other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    public Vec3f Cross(Vec3f other)
    {
        return new Vec3f(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }
}
