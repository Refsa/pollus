namespace Pollus.Mathematics;

public struct Vector2<T>
    where T : System.Numerics.INumber<T>
{
    public T X;
    public T Y;

    public Vector2(T x, T y)
    {
        X = x;
        Y = y;
    }
}

public struct Vector3<T>
    where T : struct, System.Numerics.INumber<T>
{
    public T X;
    public T Y;
    public T Z;

    public static Vector3<T> Zero => new Vector3<T>(default, default, default);


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