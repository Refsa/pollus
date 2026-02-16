namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UIRootBuilder : UINodeBuilder<UIRootBuilder>
{
    float viewportWidth;
    float viewportHeight;

    public UIRootBuilder(Commands commands, float width, float height) : base(commands)
    {
        viewportWidth = width;
        viewportHeight = height;
        style = LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
        };
    }

    bool autoResize;

    /// <summary>
    /// Automatically resize this root to match its target viewport (window by default).
    /// </summary>
    public UIRootBuilder AutoResize()
    {
        autoResize = true;
        return this;
    }

    public override Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(viewportWidth, viewportHeight) },
            new UIStyle { Value = style }
        )).Entity;

        if (autoResize)
            commands.Entity(entity).AddComponent(new UIAutoResize());

        Setup(entity);

        return entity;
    }
}
