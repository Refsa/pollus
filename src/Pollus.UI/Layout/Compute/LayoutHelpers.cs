namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;
using ECS;
using Mathematics;

public static class LayoutHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<float> ResolvePadding(in Style style, Size<float?> parentSize)
    {
        float parentWidth = parentSize.Width ?? 0f;
        return new Rect<float>(
            style.Padding.Left.ResolveOr(parentWidth, 0f),
            style.Padding.Right.ResolveOr(parentWidth, 0f),
            style.Padding.Top.ResolveOr(parentWidth, 0f),
            style.Padding.Bottom.ResolveOr(parentWidth, 0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<float> ResolveBorder(in Style style, Size<float?> parentSize)
    {
        float parentWidth = parentSize.Width ?? 0f;
        return new Rect<float>(
            style.Border.Left.ResolveOr(parentWidth, 0f),
            style.Border.Right.ResolveOr(parentWidth, 0f),
            style.Border.Top.ResolveOr(parentWidth, 0f),
            style.Border.Bottom.ResolveOr(parentWidth, 0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<float> ResolveMargin(in Style style, Size<float?> parentSize)
    {
        float parentWidth = parentSize.Width ?? 0f;
        return new Rect<float>(
            style.Margin.Left.ResolveOr(parentWidth, 0f),
            style.Margin.Right.ResolveOr(parentWidth, 0f),
            style.Margin.Top.ResolveOr(parentWidth, 0f),
            style.Margin.Bottom.ResolveOr(parentWidth, 0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<float?> ResolveInset(in Style style, Size<float?> parentSize)
    {
        float pw = parentSize.Width ?? 0f;
        float ph = parentSize.Height ?? 0f;
        return new Rect<float?>(
            style.Inset.Left.Resolve(pw),
            style.Inset.Right.Resolve(pw),
            style.Inset.Top.Resolve(ph),
            style.Inset.Bottom.Resolve(ph)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? MaybeClamp(float? value, float? min, float? max)
    {
        if (!value.HasValue) return null;
        float v = value.Value;
        if (min.HasValue && v < min.Value) v = min.Value;
        if (max.HasValue && v > max.Value) v = max.Value;
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? MaybeMax(float? a, float? b)
    {
        if (!a.HasValue) return b;
        if (!b.HasValue) return a;
        return MathF.Max(a.Value, b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? MaybeMin(float? a, float? b)
    {
        if (!a.HasValue) return b;
        if (!b.HasValue) return a;
        return MathF.Min(a.Value, b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> ContentBoxAdjustment(BoxSizing boxSizing, Rect<float> padding, Rect<float> border)
    {
        if (boxSizing == BoxSizing.BorderBox)
        {
            return new Size<float?>(
                -(padding.HorizontalAxisSum() + border.HorizontalAxisSum()),
                -(padding.VerticalAxisSum() + border.VerticalAxisSum())
            );
        }

        return new Size<float?>(0f, 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? MaybeAdd(float? a, float? b)
    {
        if (!a.HasValue || !b.HasValue) return null;
        return a.Value + b.Value;
    }

    public static Vec2f ComputeAbsolutePosition(Query<ComputedNode, Child> query, Entity entity)
    {
        var pos = Vec2f.Zero;
        var current = entity;
        while (!current.IsNull)
        {
            if (query.Has<ComputedNode>(current))
                pos += query.Get<ComputedNode>(current).Position;

            if (query.Has<Child>(current))
                current = query.Get<Child>(current).Parent;
            else
                break;
        }

        return pos;
    }

    public static float ComputeValueFromPosition(float mouseX, float trackAbsX, float trackWidth, in UISlider slider)
    {
        if (trackWidth <= 0) return slider.Min;

        var relativeX = mouseX - trackAbsX;
        var ratio = Math.Clamp(relativeX / trackWidth, 0f, 1f);
        var value = slider.Min + ratio * (slider.Max - slider.Min);

        // Snap to step
        if (slider.Step > 0)
        {
            value = MathF.Round(value / slider.Step) * slider.Step;
        }

        return Math.Clamp(value, slider.Min, slider.Max);
    }
}
