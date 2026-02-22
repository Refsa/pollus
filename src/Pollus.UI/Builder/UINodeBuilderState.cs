namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UINodeBuilderState
{
    internal Commands commands;
    internal LayoutStyle style;
    internal Color? backgroundColor;
    internal BorderColor? borderColor;
    internal BorderRadius? borderRadius;
    internal BoxShadow? boxShadow;
    internal Entity? parentEntity;
    internal List<Entity>? children;
    internal bool focusable;
    internal bool interactable;
    internal UIShapeType? shape;
    internal Outline? outline;
    internal bool noFocusVisual;
    internal Handle? material;

    internal UINodeBuilderState(Commands commands)
    {
        this.commands = commands;
        style = LayoutStyle.Default;
    }

    internal void Setup(Entity entity)
    {
        AddComponents(entity);
        AddVisualComponents(entity);
        SetupHierarchy(entity);
    }

    internal void AddComponents(Entity entity)
    {
        if (interactable)
        {
            commands.AddComponent(entity, new UIInteraction { Focusable = focusable });
        }

        if (focusable && !outline.HasValue)
        {
            commands.AddComponent(entity, new Pollus.UI.Outline());
        }

        if (noFocusVisual)
        {
            commands.AddComponent(entity, new UIFocusVisual { Disabled = true });
        }

        if (style.Overflow.X is Layout.Overflow.Scroll || style.Overflow.Y is Layout.Overflow.Scroll)
        {
            commands.AddComponent(entity, new UIScrollOffset());
        }
    }

    internal void AddVisualComponents(Entity entity)
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

        if (outline.HasValue)
            commands.AddComponent(entity, outline.Value);

        if (material.HasValue)
            commands.AddComponent(entity, new UIMaterial { Material = material.Value });
    }

    internal void SetupHierarchy(Entity entity)
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
