using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UICheckBoxTests
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
    public void Click_TogglesIsChecked()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var checkBox = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UICheckBox { IsChecked = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(24), Length.Px(24)) } }
        )).Entity;

        commands.AddChild(root, checkBox);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = checkBox });

        var query = new Query<UICheckBox, UIInteraction, BackgroundColor>(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UICheckBoxSystem.UpdateCheckBoxes(query, clickReader, world.Events);

        var state = world.Store.GetComponent<UICheckBox>(checkBox);
        Assert.True(state.IsChecked);
    }

    [Fact]
    public void Click_UnchecksWhenChecked()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var checkBox = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UICheckBox { IsChecked = true },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(24), Length.Px(24)) } }
        )).Entity;

        commands.AddChild(root, checkBox);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = checkBox });

        var query = new Query<UICheckBox, UIInteraction, BackgroundColor>(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UICheckBoxSystem.UpdateCheckBoxes(query, clickReader, world.Events);

        var state = world.Store.GetComponent<UICheckBox>(checkBox);
        Assert.False(state.IsChecked);
    }

    [Fact]
    public void UICheckBoxEvent_EmittedWithCorrectState()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var checkBox = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UICheckBox { IsChecked = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(24), Length.Px(24)) } }
        )).Entity;

        commands.AddChild(root, checkBox);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = checkBox });

        var query = new Query<UICheckBox, UIInteraction, BackgroundColor>(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var cbReader = world.Events.GetReader<UICheckBoxEvents.UICheckBoxEvent>()!;

        UICheckBoxSystem.UpdateCheckBoxes(query, clickReader, world.Events);

        var events = cbReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(checkBox, events[0].Entity);
        Assert.True(events[0].IsChecked);
    }

    [Fact]
    public void BackgroundColor_UpdatesOnStateChange()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var checkedColor = new Color(0f, 1f, 0f, 1f);
        var uncheckedColor = new Color(1f, 0f, 0f, 1f);

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var checkBox = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UICheckBox { IsChecked = false, CheckedColor = checkedColor, UncheckedColor = uncheckedColor },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(24), Length.Px(24)) } }
        )).Entity;

        commands.AddChild(root, checkBox);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = checkBox });

        var query = new Query<UICheckBox, UIInteraction, BackgroundColor>(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UICheckBoxSystem.UpdateCheckBoxes(query, clickReader, world.Events);

        var bg = world.Store.GetComponent<BackgroundColor>(checkBox);
        Assert.Equal(checkedColor, bg.Color);
    }

    [Fact]
    public void DisabledCheckBox_DoesNotToggle()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var checkBox = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true, State = InteractionState.Disabled },
            new UICheckBox { IsChecked = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(24), Length.Px(24)) } }
        )).Entity;

        commands.AddChild(root, checkBox);
        world.Update();

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = checkBox });

        var query = new Query<UICheckBox, UIInteraction, BackgroundColor>(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UICheckBoxSystem.UpdateCheckBoxes(query, clickReader, world.Events);

        var state = world.Store.GetComponent<UICheckBox>(checkBox);
        Assert.False(state.IsChecked);
    }
}
