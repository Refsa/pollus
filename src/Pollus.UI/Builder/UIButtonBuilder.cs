namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UIButtonBuilder : IUINodeBuilder<UIButtonBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UIButton button;

    public UIButtonBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        button = new();
    }

    public UIButtonBuilder Colors(Color normal, Color? hover = null, Color? pressed = null, Color? disabled = null)
    {
        button.NormalColor = normal;
        if (hover.HasValue) button.HoverColor = hover.Value;
        if (pressed.HasValue) button.PressedColor = pressed.Value;
        if (disabled.HasValue) button.DisabledColor = disabled.Value;
        return this;
    }

    public Entity Spawn()
    {
        state.interactable = true;
        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            button,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return entity;
    }
}
