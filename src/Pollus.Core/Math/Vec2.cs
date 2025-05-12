namespace Pollus.Mathematics;

using System.Runtime.CompilerServices;
using Pollus.Graphics;

[ShaderType]
public partial record struct Vec2<T>
    where T : struct, System.Numerics.INumber<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 2;

    public static Vec2<T> Zero => new(default, default);
    public static Vec2<T> One => new(T.One, T.One);
    public static Vec2<T> Up => new(T.Zero, T.One);
    public static Vec2<T> Down => new(T.Zero, T.CreateChecked(-1));
    public static Vec2<T> Left => new(T.CreateChecked(-1), T.Zero);
    public static Vec2<T> Right => new(T.One, T.Zero);


    public T X;
    public T Y;

    public Vec2(T x, T y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vec2<T>(System.Numerics.Vector2 vector2)
    {
        return new Vec2<T>((T)Convert.ChangeType(vector2.X, typeof(T)), (T)Convert.ChangeType(vector2.Y, typeof(T)));
    }

    public static implicit operator System.Numerics.Vector2(Vec2<T> vector2)
    {
        return new System.Numerics.Vector2((float)Convert.ChangeType(vector2.X, typeof(float)), (float)Convert.ChangeType(vector2.Y, typeof(float)));
    }

    public static implicit operator Vec2<T>((T, T) tuple)
    {
        return new Vec2<T>(tuple.Item1, tuple.Item2);
    }

    public static Vec2<T> operator +(Vec2<T> left, Vec2<T> right)
    {
        return new Vec2<T>(left.X + right.X, left.Y + right.Y);
    }

    public static Vec2<T> operator -(Vec2<T> left, Vec2<T> right)
    {
        return new Vec2<T>(left.X - right.X, left.Y - right.Y);
    }

    public static Vec2<T> operator *(Vec2<T> left, T right)
    {
        return new Vec2<T>(left.X * right, left.Y * right);
    }

    public static Vec2<T> operator *(T left, Vec2<T> right)
    {
        return new Vec2<T>(left * right.X, left * right.Y);
    }

    public static Vec2<T> operator *(Vec2<T> left, Vec2<T> right)
    {
        return new Vec2<T>(left.X * right.X, left.Y * right.Y);
    }

    public static Vec2<T> operator /(Vec2<T> left, T right)
    {
        return new Vec2<T>(left.X / right, left.Y / right);
    }
}
