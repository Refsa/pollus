namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;

public readonly record struct Length
{
    public enum Kind : byte { Px, Percent, Auto }

    public Kind Tag { get; init; }
    public float Value { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Px(float v) => new() { Tag = Kind.Px, Value = v };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Length Percent(float v) => new() { Tag = Kind.Percent, Value = v };

    public static readonly Length Auto = new() { Tag = Kind.Auto };
    public static readonly Length Zero = Px(0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Length(float px) => Px(px);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Length(int px) => Px(px);

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
