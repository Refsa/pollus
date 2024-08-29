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
    public const double DEG2RAD = 0.0174532925;
    public const double RAD2DEG = 57.2957795;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Sign<T>(T self)
        where T : struct, INumber<T>
    {
        return T.Sign(self);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Min<T>(T a, T b)
        where T : struct, INumber<T>
    {
        return a.CompareTo(b) < 0 ? a : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Max<T>(T a, T b)
        where T : struct, INumber<T>
    {
        return a.CompareTo(b) > 0 ? a : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Abs<T>(T self)
        where T : struct, INumber<T>
    {
        return T.Abs(self);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Radians<T>(this T degrees)
        where T : struct, IFloatingPoint<T>
    {
        return degrees * FloatingConstants<T>.DEG2RAD;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Degrees<T>(this T radians)
        where T : struct, IFloatingPoint<T>
    {
        return radians * FloatingConstants<T>.RAD2DEG;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static (T Sin, T Cos) SinCos<T>(this T radians)
        where T : struct, IFloatingPoint<T>, ITrigonometricFunctions<T>
    {
        return (radians.Sin(), radians.Cos());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Sin<T>(this T radians)
        where T : struct, IFloatingPoint<T>, ITrigonometricFunctions<T>
    {
        return T.Sin(radians);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Cos<T>(this T radians)
        where T : struct, IFloatingPoint<T>, ITrigonometricFunctions<T>
    {
        return T.Cos(radians);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Sqrt<T>(this T self)
        where T : struct, INumber<T>, IRootFunctions<T>
    {
        return T.Sqrt(self);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Rcp<T>(this T self)
        where T : struct, IFloatingPoint<T>
    {
        return T.One / self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Wrap<T>(this T value, T min, T max)
        where T : struct, INumber<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return max - (min - value) % (max - min);
        }
        else
        {
            return min + (value - min) % (max - min);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Clamp<T>(this T value, T min, T max)
        where T : struct, INumber<T>
    {
        return Min(Max(value, min), max);
    }
}