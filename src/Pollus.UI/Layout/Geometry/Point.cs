using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public record struct Point<T>
{
    public T X;
    public T Y;

    public Point(T x, T y)
    {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Main(FlexDirection direction) => direction.IsRow() ? X : Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Cross(FlexDirection direction) => direction.IsRow() ? Y : X;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Point<T> WithMain(FlexDirection direction, T value) =>
        direction.IsRow() ? new(value, Y) : new(X, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Point<T> WithCross(FlexDirection direction, T value) =>
        direction.IsRow() ? new(X, value) : new(value, Y);

    public static readonly Point<T> Zero = default;
}
