namespace Pollus.UI.Layout;

public record struct Style
{
    // Display & positioning
    public Display Display;
    public Position Position;
    public BoxSizing BoxSizing;
    public Point<Overflow> Overflow;

    // Sizing
    public Size<Length> Size;
    public Size<Length> MinSize;
    public Size<Length> MaxSize;
    public float? AspectRatio;

    // Spacing
    public Rect<Length> Margin;
    public Rect<Length> Padding;
    public Rect<Length> Border;
    public Rect<Length> Inset;

    // Flex container
    public FlexDirection FlexDirection;
    public FlexWrap FlexWrap;
    public Size<Length> Gap;

    // Flex item
    public float FlexGrow;
    public float FlexShrink;
    public Length FlexBasis;

    // Alignment (nullable = "not set", distinct from "set to default")
    public AlignItems? AlignItems;
    public AlignSelf? AlignSelf;
    public AlignContent? AlignContent;
    public JustifyContent? JustifyContent;
    public JustifySelf? JustifySelf;

    // CSS order
    public int Order;

    public static Style Default => new()
    {
        Display = Display.Flex,
        Position = Position.Relative,
        BoxSizing = BoxSizing.BorderBox,
        Overflow = new Point<Overflow>(Layout.Overflow.Visible, Layout.Overflow.Visible),
        Size = new Size<Length>(Length.Auto, Length.Auto),
        MinSize = new Size<Length>(Length.Auto, Length.Auto),
        MaxSize = new Size<Length>(Length.Auto, Length.Auto),
        AspectRatio = null,
        Margin = Rect<Length>.Zero,
        Padding = Rect<Length>.Zero,
        Border = Rect<Length>.Zero,
        Inset = new Rect<Length>(
            Length.Auto,
            Length.Auto,
            Length.Auto,
            Length.Auto
        ),
        FlexDirection = FlexDirection.Row,
        FlexWrap = FlexWrap.NoWrap,
        Gap = Size<Length>.Zero,
        FlexGrow = 0f,
        FlexShrink = 1f,
        FlexBasis = Length.Auto,
        AlignItems = null,
        AlignSelf = null,
        AlignContent = null,
        JustifyContent = null,
        JustifySelf = null,
        Order = 0,
    };
}
