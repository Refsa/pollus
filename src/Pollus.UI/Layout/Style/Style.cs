namespace Pollus.UI.Layout;

public record struct Style
{
    // Display & positioning
    public Display Display;
    public Position Position;
    public BoxSizing BoxSizing;
    public Point<Overflow> Overflow;

    // Sizing
    public Size<Dimension> Size;
    public Size<Dimension> MinSize;
    public Size<Dimension> MaxSize;
    public float? AspectRatio;

    // Spacing
    public Rect<LengthAuto> Margin;
    public Rect<Length> Padding;
    public Rect<Length> Border;
    public Rect<LengthAuto> Inset;

    // Flex container
    public FlexDirection FlexDirection;
    public FlexWrap FlexWrap;
    public Size<Length> Gap;

    // Flex item
    public float FlexGrow;
    public float FlexShrink;
    public Dimension FlexBasis;

    // Alignment (nullable = "not set", distinct from "set to default")
    public AlignItems? AlignItems;
    public AlignSelf? AlignSelf;
    public AlignContent? AlignContent;
    public JustifyContent? JustifyContent;

    // CSS order
    public int Order;

    public static Style Default => new()
    {
        Display = Display.Flex,
        Position = Position.Relative,
        BoxSizing = BoxSizing.BorderBox,
        Overflow = new Point<Overflow>(Layout.Overflow.Visible, Layout.Overflow.Visible),
        Size = new Size<Dimension>(Dimension.Auto, Dimension.Auto),
        MinSize = new Size<Dimension>(Dimension.Auto, Dimension.Auto),
        MaxSize = new Size<Dimension>(Dimension.Auto, Dimension.Auto),
        AspectRatio = null,
        Margin = Rect<LengthAuto>.Zero,
        Padding = Rect<Length>.Zero,
        Border = Rect<Length>.Zero,
        Inset = new Rect<LengthAuto>(
            LengthAuto.Auto,
            LengthAuto.Auto,
            LengthAuto.Auto,
            LengthAuto.Auto
        ),
        FlexDirection = FlexDirection.Row,
        FlexWrap = FlexWrap.NoWrap,
        Gap = Size<Length>.Zero,
        FlexGrow = 0f,
        FlexShrink = 1f,
        FlexBasis = Dimension.Auto,
        AlignItems = null,
        AlignSelf = null,
        AlignContent = null,
        JustifyContent = null,
        Order = 0,
    };
}
