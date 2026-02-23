namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

public static class UINodeBuilderExtensions
{
    // Layout: Size

    public static TSelf Size<TSelf>(this TSelf self, Length width, Length height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(width, height) };
        return self;
    }

    public static TSelf Size<TSelf>(this TSelf self, float width, float height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return self;
    }

    public static TSelf Width<TSelf>(this TSelf self, float width)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(Length.Px(width), s.style.Size.Height) };
        return self;
    }

    public static TSelf Height<TSelf>(this TSelf self, float height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(s.style.Size.Width, Length.Px(height)) };
        return self;
    }

    public static TSelf WidthPercent<TSelf>(this TSelf self, float percent)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(Length.Percent(percent), s.style.Size.Height) };
        return self;
    }

    public static TSelf HeightPercent<TSelf>(this TSelf self, float percent)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(s.style.Size.Width, Length.Percent(percent)) };
        return self;
    }

    public static TSelf SizePercent<TSelf>(this TSelf self, float width, float height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Size = new Size<Length>(Length.Percent(width), Length.Percent(height)) };
        return self;
    }

    public static TSelf MinSize<TSelf>(this TSelf self, Length width, Length height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { MinSize = new Size<Length>(width, height) };
        return self;
    }

    public static TSelf MinSize<TSelf>(this TSelf self, float width, float height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { MinSize = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return self;
    }

    public static TSelf MaxSize<TSelf>(this TSelf self, Length width, Length height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { MaxSize = new Size<Length>(width, height) };
        return self;
    }

    public static TSelf MaxSize<TSelf>(this TSelf self, float width, float height)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { MaxSize = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return self;
    }

    // Layout: Flex direction shortcuts

    public static TSelf FlexRow<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { FlexDirection = FlexDirection.Row };
        return self;
    }

    public static TSelf FlexColumn<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { FlexDirection = FlexDirection.Column };
        return self;
    }

    public static TSelf FlexWrap<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { FlexWrap = Layout.FlexWrap.Wrap };
        return self;
    }

    public static TSelf FlexGrow<TSelf>(this TSelf self, float value)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { FlexGrow = value };
        return self;
    }

    public static TSelf FlexShrink<TSelf>(this TSelf self, float value)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { FlexShrink = value };
        return self;
    }

    // Layout: Gap

    public static TSelf Gap<TSelf>(this TSelf self, float gap)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Gap = new Size<Length>(Length.Px(gap), Length.Px(gap)) };
        return self;
    }

    public static TSelf Gap<TSelf>(this TSelf self, float row, float column)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Gap = new Size<Length>(Length.Px(row), Length.Px(column)) };
        return self;
    }

    // Layout: Padding

    public static TSelf Padding<TSelf>(this TSelf self, float all)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Padding = Rect<Length>.All(Length.Px(all)) };
        return self;
    }

    public static TSelf Padding<TSelf>(this TSelf self, float top, float right, float bottom, float left)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with
        {
            Padding = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return self;
    }

    // Layout: Margin

    public static TSelf Margin<TSelf>(this TSelf self, float all)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Margin = Rect<Length>.All(Length.Px(all)) };
        return self;
    }

    public static TSelf Margin<TSelf>(this TSelf self, float top, float right, float bottom, float left)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with
        {
            Margin = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return self;
    }

    // Layout: Border (layout border, not visual)

    public static TSelf Border<TSelf>(this TSelf self, float all)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Border = Rect<Length>.All(Length.Px(all)) };
        return self;
    }

    // Layout: Alignment

    public static TSelf AlignItems<TSelf>(this TSelf self, AlignItems align)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { AlignItems = align };
        return self;
    }

    public static TSelf JustifyContent<TSelf>(this TSelf self, JustifyContent justify)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { JustifyContent = justify };
        return self;
    }

    public static TSelf AlignSelf<TSelf>(this TSelf self, AlignSelf align)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { AlignSelf = align };
        return self;
    }

    public static TSelf JustifySelf<TSelf>(this TSelf self, JustifySelf justify)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { JustifySelf = justify };
        return self;
    }

    // Layout: Position

    public static TSelf PositionAbsolute<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Position = Position.Absolute };
        return self;
    }

    public static TSelf Inset<TSelf>(this TSelf self, float top, float right, float bottom, float left)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with
        {
            Inset = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return self;
    }

    // Layout: Display

    public static TSelf DisplayNone<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Display = Display.None };
        return self;
    }

    // Layout: Overflow

    public static TSelf Overflow<TSelf>(this TSelf self, Overflow x, Overflow y)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = s.style with { Overflow = new Point<Overflow>(x, y) };
        return self;
    }

    // Interaction

    public static TSelf Focusable<TSelf>(this TSelf self, bool value = true)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.focusable = value;
        return self;
    }

    public static TSelf Interactable<TSelf>(this TSelf self, bool value = true)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.interactable = value;
        return self;
    }

    // Layout: Raw style access

    public static TSelf Style<TSelf>(this TSelf self, LayoutStyle value)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.style = value;
        return self;
    }

    public static TSelf Style<TSelf>(this TSelf self, Func<LayoutStyle, LayoutStyle> configure)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.style = configure(s.style);
        return self;
    }

    // Visuals

    public static TSelf Background<TSelf>(this TSelf self, Color color)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.backgroundColor = color;
        return self;
    }

    public static TSelf BorderColor<TSelf>(this TSelf self, Color color)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.borderColor = new Pollus.UI.BorderColor { Top = color, Right = color, Bottom = color, Left = color };
        return self;
    }

    public static TSelf BorderColor<TSelf>(this TSelf self, Color top, Color right, Color bottom, Color left)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.borderColor = new Pollus.UI.BorderColor { Top = top, Right = right, Bottom = bottom, Left = left };
        return self;
    }

    public static TSelf BorderRadius<TSelf>(this TSelf self, float all)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.borderRadius = new Pollus.UI.BorderRadius { TopLeft = all, TopRight = all, BottomRight = all, BottomLeft = all };
        return self;
    }

    public static TSelf BorderRadius<TSelf>(this TSelf self, float topLeft, float topRight, float bottomRight, float bottomLeft)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.borderRadius = new Pollus.UI.BorderRadius
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomRight = bottomRight,
            BottomLeft = bottomLeft
        };
        return self;
    }

    public static TSelf Shadow<TSelf>(this TSelf self, float offsetX, float offsetY, float blur, float spread, Color color)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.boxShadow = new Pollus.UI.BoxShadow
        {
            Offset = new Mathematics.Vec2f(offsetX, offsetY),
            Blur = blur,
            Spread = spread,
            Color = color
        };
        return self;
    }

    public static TSelf Shape<TSelf>(this TSelf self, UIShapeType? type)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.shape = type;
        return self;
    }

    public static TSelf Outline<TSelf>(this TSelf self, Utils.Color color, float width, float offset = 0f)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.outline = new Pollus.UI.Outline { Color = color, Width = width, Offset = offset };
        return self;
    }

    public static TSelf NoFocusVisual<TSelf>(this TSelf self)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.noFocusVisual = true;
        return self;
    }

    public static TSelf Material<TSelf>(this TSelf self, Handle handle)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.material = handle;
        return self;
    }

    // Hierarchy

    public static TSelf ChildOf<TSelf>(this TSelf self, Entity parent)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        self.State.parentEntity = parent;
        return self;
    }

    public static TSelf Children<TSelf>(this TSelf self, params ReadOnlySpan<Entity> childEntities)
        where TSelf : struct, IUINodeBuilder<TSelf>
    {
        ref var s = ref self.State;
        s.children ??= [];
        foreach (var child in childEntities)
            s.children.Add(child);
        return self;
    }
}
