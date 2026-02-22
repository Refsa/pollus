namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UIRootBuilder : IUINodeBuilder<UIRootBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    float viewportWidth;
    float viewportHeight;
    bool autoResize;

    public UIRootBuilder(Commands commands, float width, float height)
    {
        state = new UINodeBuilderState(commands);
        viewportWidth = width;
        viewportHeight = height;
        state.style = LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
        };
    }

    /// <summary>
    /// Automatically resize this root to match its target viewport (window by default).
    /// </summary>
    public UIRootBuilder AutoResize()
    {
        autoResize = true;
        return this;
    }

    public Entity Spawn()
    {
        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(viewportWidth, viewportHeight) },
            new UIStyle { Value = state.style }
        )).Entity;

        if (autoResize)
            state.commands.Entity(entity).AddComponent(new UIAutoResize());

        state.Setup(entity);

        return entity;
    }
}
