using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public static class RoundLayout
{
    public static void Round<TTree>(ref TTree tree, int rootId)
        where TTree : ILayoutTree
    {
        ref var rootLayout = ref tree.GetLayout(rootId);
        float absX = rootLayout.Location.X;
        float absY = rootLayout.Location.Y;

        rootLayout.Location = new Point<float>(
            MathF.Round(absX),
            MathF.Round(absY)
        );
        rootLayout.Size = RoundSize(rootLayout.Size, absX, absY);
        RoundBorderPaddingMargin(ref rootLayout);

        RoundChildren(ref tree, rootId, absX, absY);
    }

    private static void RoundChildren<TTree>(ref TTree tree, int parentId, float parentAbsX, float parentAbsY)
        where TTree : ILayoutTree
    {
        var childIds = tree.GetChildIds(parentId);
        for (int i = 0; i < childIds.Length; i++)
        {
            int childId = childIds[i];
            ref var layout = ref tree.GetLayout(childId);

            if (layout.Size.Width == 0f && layout.Size.Height == 0f
                && layout.Location.X == 0f && layout.Location.Y == 0f)
            {
                continue;
            }

            float absX = parentAbsX + layout.Location.X;
            float absY = parentAbsY + layout.Location.Y;

            float roundedAbsX = MathF.Round(absX);
            float roundedAbsY = MathF.Round(absY);

            layout.Location = new Point<float>(
                roundedAbsX - MathF.Round(parentAbsX),
                roundedAbsY - MathF.Round(parentAbsY)
            );

            layout.Size = RoundSize(layout.Size, absX, absY);
            RoundBorderPaddingMargin(ref layout);

            RoundChildren(ref tree, childId, absX, absY);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Size<float> RoundSize(Size<float> size, float absX, float absY)
    {
        return new Size<float>(
            MathF.Round(absX + size.Width) - MathF.Round(absX),
            MathF.Round(absY + size.Height) - MathF.Round(absY)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RoundBorderPaddingMargin(ref NodeLayout layout)
    {
        layout.Border = RoundRect(layout.Border);
        layout.Padding = RoundRect(layout.Padding);
        layout.Margin = RoundRect(layout.Margin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rect<float> RoundRect(Rect<float> r) => new(
        MathF.Round(r.Left),
        MathF.Round(r.Right),
        MathF.Round(r.Top),
        MathF.Round(r.Bottom)
    );
}
