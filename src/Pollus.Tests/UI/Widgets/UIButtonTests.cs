using Pollus.ECS;
using Pollus.Engine.UI;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIButtonTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Resources.Add(new UIHitTestResult());
        world.Resources.Add(new UIFocusState());
        world.Events.InitEvent<UIInteractionEvents.UIHoverEnterEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIHoverExitEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIPressEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIReleaseEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIClickEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIFocusEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIBlurEvent>();
        world.Prepare();
        return world;
    }

    [Fact]
    public void NormalState_AppliesNormalColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = new UIButton
        {
            NormalColor = new Color(0.1f, 0.2f, 0.3f, 1f),
        };
        var btn = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            button,
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, btn);
        world.Update();

        var query = new Query(world);
        UIWidgetSystems.UpdateButtonVisuals(query);

        var bg = world.Store.GetComponent<BackgroundColor>(btn);
        Assert.Equal(button.NormalColor, bg.Color);
    }

    [Fact]
    public void HoveredState_AppliesHoverColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = new UIButton
        {
            HoverColor = new Color(0.9f, 0.9f, 0.9f, 1f),
        };
        var btn = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { State = InteractionState.Hovered },
            button,
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, btn);
        world.Update();

        var query = new Query(world);
        UIWidgetSystems.UpdateButtonVisuals(query);

        var bg = world.Store.GetComponent<BackgroundColor>(btn);
        Assert.Equal(button.HoverColor, bg.Color);
    }

    [Fact]
    public void PressedState_AppliesPressedColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = new UIButton
        {
            PressedColor = new Color(0.3f, 0.3f, 0.3f, 1f),
        };
        var btn = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { State = InteractionState.Pressed | InteractionState.Hovered },
            button,
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, btn);
        world.Update();

        var query = new Query(world);
        UIWidgetSystems.UpdateButtonVisuals(query);

        var bg = world.Store.GetComponent<BackgroundColor>(btn);
        Assert.Equal(button.PressedColor, bg.Color);
    }

    [Fact]
    public void DisabledState_AppliesDisabledColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = new UIButton
        {
            DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
        };
        var btn = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { State = InteractionState.Disabled },
            button,
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, btn);
        world.Update();

        var query = new Query(world);
        UIWidgetSystems.UpdateButtonVisuals(query);

        var bg = world.Store.GetComponent<BackgroundColor>(btn);
        Assert.Equal(button.DisabledColor, bg.Color);
    }
}
