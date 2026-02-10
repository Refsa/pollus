namespace Pollus.UI.Layout;

public record struct NodeLayout
{
    public uint Order;
    public Point<float> Location;
    public Size<float> Size;
    public Size<float> ContentSize;
    public Rect<float> Border;
    public Rect<float> Padding;
    public Rect<float> Margin;
    public Size<float> ScrollbarSize;

    public static readonly NodeLayout Zero = default;
}
