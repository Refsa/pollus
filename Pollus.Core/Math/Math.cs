namespace Pollus.Mathematics;

using System.Numerics;
using System.Runtime.CompilerServices;

public static class FloatingConstants<T>
    where T : IFloatingPoint<T>
{
    public static readonly T RAD2DEG = T.CreateChecked(Math.RAD2DEG);
    public static readonly T DEG2RAD = T.CreateChecked(Math.DEG2RAD);
}

public static class Math
{
    public const float DEG2RAD = 0.0174532925f;
    public const float RAD2DEG = 57.2957795f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Deg2Rad<T>(this T deg)
        where T : struct, IFloatingPoint<T>
    {
        return deg * FloatingConstants<T>.DEG2RAD;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Rad2Deg<T>(this T rad)
        where T : struct, IFloatingPoint<T>
    {
        return rad * FloatingConstants<T>.RAD2DEG;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Sin, T Cos) SinCos<T>(this T radians)
        where T : struct, IFloatingPoint<T>
    {
        return (radians.Sin(), radians.Cos());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sin<T>(this T radians)
        where T : struct, IFloatingPoint<T>
    {
        return T.CreateChecked(System.Math.Sin(double.CreateChecked(radians)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cos<T>(this T radians)
        where T : struct, IFloatingPoint<T>
    {
        return T.CreateChecked(System.Math.Cos(double.CreateChecked(radians)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sqrt<T>(this T self)
        where T : struct, IFloatingPoint<T>
    {
        return T.CreateChecked(System.Math.Sqrt(double.CreateChecked(self)));
    }
}