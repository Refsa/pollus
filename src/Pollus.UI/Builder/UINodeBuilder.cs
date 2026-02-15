namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UINodeBuilder<TSelf> where TSelf : UINodeBuilder<TSelf>
{
    protected Commands commands;
    protected LayoutStyle style = LayoutStyle.Default;
    protected Color? backgroundColor;
    protected BorderColor? borderColor;
    protected BorderRadius? borderRadius;
    protected BoxShadow? boxShadow;
    protected Entity? parentEntity;
    protected List<Entity>? children;
    protected bool focusable;
    protected bool interactable;
    protected UIShapeType? shape;

    public UINodeBuilder(Commands commands)
    {
        this.commands = commands;
    }

    // Layout: Size
    public TSelf Size(float width, float height)
    {
        style = style with { Size = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return (TSelf)this;
    }

    public TSelf Width(float width)
    {
        style = style with { Size = new Size<Length>(Length.Px(width), style.Size.Height) };
        return (TSelf)this;
    }

    public TSelf Height(float height)
    {
        style = style with { Size = new Size<Length>(style.Size.Width, Length.Px(height)) };
        return (TSelf)this;
    }

    public TSelf WidthPercent(float percent)
    {
        style = style with { Size = new Size<Length>(Length.Percent(percent), style.Size.Height) };
        return (TSelf)this;
    }

    public TSelf HeightPercent(float percent)
    {
        style = style with { Size = new Size<Length>(style.Size.Width, Length.Percent(percent)) };
        return (TSelf)this;
    }

    public TSelf SizePercent(float width, float height)
    {
        style = style with { Size = new Size<Length>(Length.Percent(width), Length.Percent(height)) };
        return (TSelf)this;
    }

    public TSelf MinSize(float width, float height)
    {
        style = style with { MinSize = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return (TSelf)this;
    }

    public TSelf MaxSize(float width, float height)
    {
        style = style with { MaxSize = new Size<Length>(Length.Px(width), Length.Px(height)) };
        return (TSelf)this;
    }

    // Layout: Flex direction shortcuts
    public TSelf FlexRow()
    {
        style = style with { FlexDirection = FlexDirection.Row };
        return (TSelf)this;
    }

    public TSelf FlexColumn()
    {
        style = style with { FlexDirection = FlexDirection.Column };
        return (TSelf)this;
    }

    public TSelf FlexWrap()
    {
        style = style with { FlexWrap = Layout.FlexWrap.Wrap };
        return (TSelf)this;
    }

    public TSelf FlexGrow(float value)
    {
        style = style with { FlexGrow = value };
        return (TSelf)this;
    }

    public TSelf FlexShrink(float value)
    {
        style = style with { FlexShrink = value };
        return (TSelf)this;
    }

    // Layout: Gap
    public TSelf Gap(float gap)
    {
        style = style with { Gap = new Size<Length>(Length.Px(gap), Length.Px(gap)) };
        return (TSelf)this;
    }

    public TSelf Gap(float row, float column)
    {
        style = style with { Gap = new Size<Length>(Length.Px(row), Length.Px(column)) };
        return (TSelf)this;
    }

    // Layout: Padding
    public TSelf Padding(float all)
    {
        style = style with { Padding = Rect<Length>.All(Length.Px(all)) };
        return (TSelf)this;
    }

    public TSelf Padding(float top, float right, float bottom, float left)
    {
        style = style with
        {
            Padding = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return (TSelf)this;
    }

    // Layout: Margin
    public TSelf Margin(float all)
    {
        style = style with { Margin = Rect<Length>.All(Length.Px(all)) };
        return (TSelf)this;
    }

    public TSelf Margin(float top, float right, float bottom, float left)
    {
        style = style with
        {
            Margin = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return (TSelf)this;
    }

    // Layout: Border (layout border, not visual)
    public TSelf Border(float all)
    {
        style = style with { Border = Rect<Length>.All(Length.Px(all)) };
        return (TSelf)this;
    }

    // Layout: Alignment
    public TSelf AlignItems(AlignItems align)
    {
        style = style with { AlignItems = align };
        return (TSelf)this;
    }

    public TSelf JustifyContent(JustifyContent justify)
    {
        style = style with { JustifyContent = justify };
        return (TSelf)this;
    }

    public TSelf AlignSelf(AlignSelf align)
    {
        style = style with { AlignSelf = align };
        return (TSelf)this;
    }

    // Layout: Position
    public TSelf PositionAbsolute()
    {
        style = style with { Position = Position.Absolute };
        return (TSelf)this;
    }

    public TSelf Inset(float top, float right, float bottom, float left)
    {
        style = style with
        {
            Inset = new Rect<Length>(Length.Px(left), Length.Px(right), Length.Px(top), Length.Px(bottom))
        };
        return (TSelf)this;
    }

    // Layout: Display
    public TSelf DisplayNone()
    {
        style = style with { Display = Display.None };
        return (TSelf)this;
    }

    // Layout: Overflow
    public TSelf Overflow(Overflow x, Overflow y)
    {
        style = style with { Overflow = new Point<Overflow>(x, y) };
        return (TSelf)this;
    }

    // Interaction
    public TSelf Focusable(bool value = true)
    {
        focusable = value;
        return (TSelf)this;
    }

    public TSelf Interactable(bool value = true)
    {
        interactable = true;
        return (TSelf)this;
    }

    // Layout: Raw style access
    public TSelf Style(LayoutStyle value)
    {
        style = value;
        return (TSelf)this;
    }

    public TSelf Style(Func<LayoutStyle, LayoutStyle> configure)
    {
        style = configure(style);
        return (TSelf)this;
    }

    // Visuals
    public TSelf Background(Color color)
    {
        backgroundColor = color;
        return (TSelf)this;
    }

    public TSelf BorderColor(Color color)
    {
        borderColor = new Pollus.UI.BorderColor { Top = color, Right = color, Bottom = color, Left = color };
        return (TSelf)this;
    }

    public TSelf BorderColor(Color top, Color right, Color bottom, Color left)
    {
        borderColor = new Pollus.UI.BorderColor { Top = top, Right = right, Bottom = bottom, Left = left };
        return (TSelf)this;
    }

    public TSelf BorderRadius(float all)
    {
        borderRadius = new Pollus.UI.BorderRadius { TopLeft = all, TopRight = all, BottomRight = all, BottomLeft = all };
        return (TSelf)this;
    }

    public TSelf BorderRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
    {
        borderRadius = new Pollus.UI.BorderRadius
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomRight = bottomRight,
            BottomLeft = bottomLeft
        };
        return (TSelf)this;
    }

    public TSelf Shadow(float offsetX, float offsetY, float blur, float spread, Color color)
    {
        boxShadow = new Pollus.UI.BoxShadow
        {
            Offset = new Mathematics.Vec2f(offsetX, offsetY),
            Blur = blur,
            Spread = spread,
            Color = color
        };
        return (TSelf)this;
    }

    public TSelf Shape(UIShapeType? type)
    {
        shape = type;
        return (TSelf)this;
    }

    // Hierarchy
    public TSelf ChildOf(Entity parent)
    {
        parentEntity = parent;
        return (TSelf)this;
    }

    public TSelf Children(params Entity[] childEntities)
    {
        children ??= [];
        children.AddRange(childEntities);
        return (TSelf)this;
    }

    // Spawn
    public virtual Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }

    protected void Setup(Entity entity)
    {
        AddComponents(entity);
        AddVisualComponents(entity);
        SetupHierarchy(entity);
    }

    protected void AddComponents(Entity entity)
    {
        if (interactable)
        {
            commands.AddComponent(entity, new UIInteraction { Focusable = focusable });
        }

        if (style.Overflow.X is Layout.Overflow.Scroll || style.Overflow.Y is Layout.Overflow.Scroll)
        {
            commands.AddComponent(entity, new UIScrollOffset());
        }
    }

    protected void AddVisualComponents(Entity entity)
    {
        if (backgroundColor.HasValue)
            commands.AddComponent(entity, new BackgroundColor { Color = backgroundColor.Value });

        if (borderColor.HasValue)
            commands.AddComponent(entity, borderColor.Value);

        if (borderRadius.HasValue)
            commands.AddComponent(entity, borderRadius.Value);

        if (boxShadow.HasValue)
            commands.AddComponent(entity, boxShadow.Value);

        if (shape.HasValue)
            commands.AddComponent(entity, new UIShape { Type = shape.Value });
    }

    protected void SetupHierarchy(Entity entity)
    {
        if (parentEntity.HasValue)
            commands.AddChild(parentEntity.Value, entity);

        if (children != null)
        {
            foreach (var child in children)
                commands.AddChild(entity, child);
        }
    }
}

public class PanelBuilder : UINodeBuilder<PanelBuilder>
{
    public PanelBuilder(Commands commands) : base(commands) { }
}
