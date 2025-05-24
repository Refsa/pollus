namespace Pollus.Mathematics;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pollus.Graphics;

[ShaderType]
[DebuggerDisplay("Vec3: {X}, {Y}, {Z}")]
public partial record struct Vec3<T>
    where T : struct, System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 3;
    public static Vec3<T> Zero => new Vec3<T>(default, default, default);

    public T X;
    public T Y;
    public T Z;

    public Vec3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3(Vec2<T> other, T z)
    {
        X = other.X;
        Y = other.Y;
        Z = z;
    }

    public static implicit operator Vec3<T>(System.Numerics.Vector3 vector3)
    {
        return new Vec3<T>((T)Convert.ChangeType(vector3.X, typeof(T)), (T)Convert.ChangeType(vector3.Y, typeof(T)), (T)Convert.ChangeType(vector3.Z, typeof(T)));
    }

    public static implicit operator System.Numerics.Vector3(Vec3<T> vector3)
    {
        return new System.Numerics.Vector3((float)Convert.ChangeType(vector3.X, typeof(float)), (float)Convert.ChangeType(vector3.Y, typeof(float)), (float)Convert.ChangeType(vector3.Z, typeof(float)));
    }

    public static Vec3<T> operator +(Vec3<T> left, Vec3<T> right)
    {
        return new Vec3<T>(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vec3<T> operator -(Vec3<T> left, Vec3<T> right)
    {
        return new Vec3<T>(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vec3<T> operator *(Vec3<T> left, T right)
    {
        return new Vec3<T>(left.X * right, left.Y * right, left.Z * right);
    }

    public static Vec3<T> operator *(T left, Vec3<T> right)
    {
        return new Vec3<T>(left * right.X, left * right.Y, left * right.Z);
    }

    public static Vec3<T> operator *(Vec3<T> left, Vec3<T> right)
    {
        return new Vec3<T>(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }
}
