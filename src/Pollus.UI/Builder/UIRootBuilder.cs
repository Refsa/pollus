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

    public override Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(viewportWidth, viewportHeight) },
            new UIStyle { Value = style }
        )).Entity;

        AddVisualComponents(entity);
        SetupHierarchy(entity);

        return entity;
    }
}
