namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;

public struct Vector2<T>
    where T : System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 2;

    public T X;
    public T Y;

    public Vector2(T x, T y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vector2<T>(System.Numerics.Vector2 vector2)
    {
        return new Vector2<T>((T)Convert.ChangeType(vector2.X, typeof(T)), (T)Convert.ChangeType(vector2.Y, typeof(T)));
    }

    public static implicit operator System.Numerics.Vector2(Vector2<T> vector2)
    {
        return new System.Numerics.Vector2((float)Convert.ChangeType(vector2.X, typeof(float)), (float)Convert.ChangeType(vector2.Y, typeof(float)));
    }

    public static implicit operator Vector2<T>((T, T) tuple)
    {
        return new Vector2<T>(tuple.Item1, tuple.Item2);
    }
}

public struct Vector3<T>
    where T : struct, System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 3;
    public static Vector3<T> Zero => new Vector3<T>(default, default, default);

    public T X;
    public T Y;
    public T Z;

    public Vector3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static implicit operator Vector3<T>(System.Numerics.Vector3 vector3)
    {
        return new Vector3<T>((T)Convert.ChangeType(vector3.X, typeof(T)), (T)Convert.ChangeType(vector3.Y, typeof(T)), (T)Convert.ChangeType(vector3.Z, typeof(T)));
    }

    public static implicit operator System.Numerics.Vector3(Vector3<T> vector3)
    {
        return new System.Numerics.Vector3((float)Convert.ChangeType(vector3.X, typeof(float)), (float)Convert.ChangeType(vector3.Y, typeof(float)), (float)Convert.ChangeType(vector3.Z, typeof(float)));
    }
}

public struct Vector4<T>
    where T : System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 4;

    public T X;
    public T Y;
    public T Z;
    public T W;

    public Vector4(T x, T y, T z, T w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
}