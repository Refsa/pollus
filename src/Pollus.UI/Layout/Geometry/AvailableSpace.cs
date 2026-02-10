using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public readonly record struct AvailableSpace
{
    public enum Kind : byte { Definite, MinContent, MaxContent }

    public Kind Tag { get; init; }
    public float Value { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace Definite(float v) => new() { Tag = Kind.Definite, Value = v };

    public static readonly AvailableSpace MinContent = new() { Tag = Kind.MinContent };
    public static readonly AvailableSpace MaxContent = new() { Tag = Kind.MaxContent };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDefinite() => Tag == Kind.Definite;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? AsDefinite() => Tag == Kind.Definite ? Value : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AvailableSpace MaybeSet(float? value) =>
        value.HasValue ? Definite(value.Value) : this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float UnwrapOr(float fallback) => Tag == Kind.Definite ? Value : fallback;
}
