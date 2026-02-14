using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIToggleTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UISystemsPlugin(), addDependencies: true);
        world.Resources.Add(new CurrentDevice<Mouse>());
        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new ButtonInput<Key>());
        world.Prepare();
        return world;
    }

    [Fact]
    public void Click_TogglesOnFromFalse()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var toggle = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIToggle { IsOn = false, OnColor = new Color(0f, 1f, 0f, 1f), OffColor = new Color(1f, 0f, 0f, 1f) },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(100), Length.Px(50)) } }
        )).Entity;

        commands.AddChild(root, toggle);
        world.Update();

        // Emit a click event
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = toggle });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var toggleReader = world.Events.GetReader<UIToggleEvents.UIToggleEvent>()!;

        UIToggleSystem.UpdateToggles(query, clickReader, world.Events);

        var toggleState = world.Store.GetComponent<UIToggle>(toggle);
        Assert.True(toggleState.IsOn);

        var toggleEvents = toggleReader.Read();
        Assert.Equal(1, toggleEvents.Length);
        Assert.Equal(toggle, toggleEvents[0].Entity);
        Assert.True(toggleEvents[0].IsOn);

        var bg = world.Store.GetComponent<BackgroundColor>(toggle);
        Assert.Equal(new Color(0f, 1f, 0f, 1f), bg.Color);
    }

    [Fact]
    public void Click_TogglesOffFromTrue()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var toggle = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIToggle { IsOn = true, OnColor = new Color(0f, 1f, 0f, 1f), OffColor = new Color(1f, 0f, 0f, 1f) },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(100), Length.Px(50)) } }
        )).Entity;

        commands.AddChild(root, toggle);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = toggle });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var toggleReader = world.Events.GetReader<UIToggleEvents.UIToggleEvent>()!;

        UIToggleSystem.UpdateToggles(query, clickReader, world.Events);

        var toggleState = world.Store.GetComponent<UIToggle>(toggle);
        Assert.False(toggleState.IsOn);

        var toggleEvents = toggleReader.Read();
        Assert.Equal(1, toggleEvents.Length);
        Assert.False(toggleEvents[0].IsOn);

        var bg = world.Store.GetComponent<BackgroundColor>(toggle);
        Assert.Equal(new Color(1f, 0f, 0f, 1f), bg.Color);
    }

    [Fact]
    public void BackgroundColor_MatchesOnOffColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // Start as off
        var toggle = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIToggle { IsOn = false, OnColor = new Color(0f, 1f, 0f, 1f), OffColor = new Color(1f, 0f, 0f, 1f) },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(100), Length.Px(50)) } }
        )).Entity;

        commands.AddChild(root, toggle);
        world.Update();

        // First click → on
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = toggle });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UIToggleSystem.UpdateToggles(query, clickReader, world.Events);

        var bg = world.Store.GetComponent<BackgroundColor>(toggle);
        Assert.Equal(new Color(0f, 1f, 0f, 1f), bg.Color);

        // Second click → off
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = toggle });
        UIToggleSystem.UpdateToggles(query, clickReader, world.Events);

        bg = world.Store.GetComponent<BackgroundColor>(toggle);
        Assert.Equal(new Color(1f, 0f, 0f, 1f), bg.Color);
    }
}
