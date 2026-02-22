namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UIToggleBuilder : IUINodeBuilder<UIToggleBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UIToggle toggle;

    public UIToggleBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        toggle = new();
    }

    public UIToggleBuilder IsOn(bool value = true)
    {
        toggle.IsOn = value;
        return this;
    }

    public UIToggleBuilder OnColor(Color color)
    {
        toggle.OnColor = color;
        return this;
    }

    public UIToggleBuilder OffColor(Color color)
    {
        toggle.OffColor = color;
        return this;
    }

    public Entity Spawn()
    {
        state.interactable = true;
        state.focusable = true;
        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            toggle,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return entity;
    }
}
