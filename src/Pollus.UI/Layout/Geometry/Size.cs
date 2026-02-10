using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public record struct Size<T>
{
    public T Width;
    public T Height;

    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Main(FlexDirection direction) => direction.IsRow() ? Width : Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Cross(FlexDirection direction) => direction.IsRow() ? Height : Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Size<T> WithMain(FlexDirection direction, T value) =>
        direction.IsRow() ? new(value, Height) : new(Width, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Size<T> WithCross(FlexDirection direction, T value) =>
        direction.IsRow() ? new(Width, value) : new(value, Height);

    public static Size<T> FromMainCross(FlexDirection direction, T main, T cross) =>
        direction.IsRow() ? new(main, cross) : new(cross, main);

    public static readonly Size<T> Zero = default;
}

public static class SizeExtensions
{
    public static Size<float> MaybeResolve(this Size<Dimension> self, Size<float?> parentSize)
    {
        return new Size<float>(
            self.Width.Resolve(parentSize.Width ?? 0f).GetValueOrDefault(0f),
            self.Height.Resolve(parentSize.Height ?? 0f).GetValueOrDefault(0f)
        );
    }

    public static Size<float?> MaybeResolveNullable(this Size<Dimension> self, Size<float?> parentSize)
    {
        return new Size<float?>(
            self.Width.Resolve(parentSize.Width ?? 0f),
            self.Height.Resolve(parentSize.Height ?? 0f)
        );
    }
}
