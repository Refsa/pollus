using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public readonly record struct Length
{
    public enum Kind : byte { Px, Percent }

    public Kind Tag { get; init; }
    public float Value { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Px(float v) => new() { Tag = Kind.Px, Value = v };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Percent(float v) => new() { Tag = Kind.Percent, Value = v };

    public static readonly Length Zero = Px(0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Resolve(float parentSize) => Tag switch
    {
        Kind.Px => Value,
        Kind.Percent => Value * parentSize,
        _ => 0f,
    };
}

public readonly record struct LengthAuto
{
    public enum Kind : byte { Px, Percent, Auto }

    public Kind Tag { get; init; }
    public float Value { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LengthAuto Px(float v) => new() { Tag = Kind.Px, Value = v };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LengthAuto Percent(float v) => new() { Tag = Kind.Percent, Value = v };

    public static readonly LengthAuto Auto = new() { Tag = Kind.Auto };
    public static readonly LengthAuto Zero = Px(0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAuto() => Tag == Kind.Auto;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? Resolve(float parentSize) => Tag switch
    {
        Kind.Px => Value,
        Kind.Percent => Value * parentSize,
        _ => null,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ResolveOr(float parentSize, float fallback) => Resolve(parentSize) ?? fallback;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator LengthAuto(Length lp) =>
        new() { Tag = (Kind)lp.Tag, Value = lp.Value };
}

public readonly record struct Dimension
{
    public enum Kind : byte { Px, Percent, Auto }

    public Kind Tag { get; init; }
    public float Value { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dimension Px(float v) => new() { Tag = Kind.Px, Value = v };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dimension Percent(float v) => new() { Tag = Kind.Percent, Value = v };

    public static readonly Dimension Auto = new() { Tag = Kind.Auto };
    public static readonly Dimension Zero = Px(0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAuto() => Tag == Kind.Auto;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? Resolve(float parentSize) => Tag switch
    {
        Kind.Px => Value,
        Kind.Percent => Value * parentSize,
        _ => null,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ResolveOr(float parentSize, float fallback) => Resolve(parentSize) ?? fallback;
}
