namespace Pollus.Engine.Mathematics;

using System.Numerics;

public struct Vector2<T>
    where T : INumber<T>
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
    where T : INumber<T>
{
    public T X;
    public T Y;
    public T Z;

    public Vector3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public struct Vector4<T>
    where T : INumber<T>
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