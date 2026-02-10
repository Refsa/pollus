namespace Pollus.UI.Layout;

public record struct LayoutOutput
{
    public Size<float> Size;
    public Size<float> ContentSize;
    public Point<float?> FirstBaselines;

    public static readonly LayoutOutput Zero = default;

    public static LayoutOutput FromOuterSize(Size<float> size) => new() { Size = size };
}
