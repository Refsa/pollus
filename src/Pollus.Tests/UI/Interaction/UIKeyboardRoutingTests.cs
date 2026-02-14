using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Interaction;

public class UIKeyboardRoutingTests
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

    static Entity SpawnFocusableNode(World world)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();
        return child;
    }

    [Fact]
    public void KeyDown_RoutedToFocusedEntity()
    {
        using var world = CreateWorld();
        var child = SpawnFocusableNode(world);

        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child);

        // Write a key event
        var keyWriter = world.Events.GetWriter<ButtonEvent<Key>>();
        keyWriter.Write(new ButtonEvent<Key> { DeviceId = Guid.Empty, Button = Key.KeyA, State = ButtonState.JustPressed });

        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        UIKeyboardRoutingSystem.PerformRouting(keyReader, textReader, focusState, world.Events);

        var uiKeyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var events = uiKeyReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
        Assert.Equal((int)Key.KeyA, events[0].Key);
    }

    [Fact]
    public void KeyUp_RoutedToFocusedEntity()
    {
        using var world = CreateWorld();
        var child = SpawnFocusableNode(world);

        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child);

        var keyWriter = world.Events.GetWriter<ButtonEvent<Key>>();
        keyWriter.Write(new ButtonEvent<Key> { DeviceId = Guid.Empty, Button = Key.KeyA, State = ButtonState.JustReleased });

        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        UIKeyboardRoutingSystem.PerformRouting(keyReader, textReader, focusState, world.Events);

        var uiKeyReader = world.Events.GetReader<UIInteractionEvents.UIKeyUpEvent>()!;
        var events = uiKeyReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
        Assert.Equal((int)Key.KeyA, events[0].Key);
    }

    [Fact]
    public void KeyEvents_NotRouted_WhenNoFocus()
    {
        using var world = CreateWorld();
        SpawnFocusableNode(world);

        var focusState = world.Resources.Get<UIFocusState>();

        var keyWriter = world.Events.GetWriter<ButtonEvent<Key>>();
        keyWriter.Write(new ButtonEvent<Key> { DeviceId = Guid.Empty, Button = Key.KeyA, State = ButtonState.JustPressed });

        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        UIKeyboardRoutingSystem.PerformRouting(keyReader, textReader, focusState, world.Events);

        var uiKeyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var events = uiKeyReader.Read();
        Assert.Equal(0, events.Length);
    }

    [Fact]
    public void TextInput_RoutedToFocusedEntity()
    {
        using var world = CreateWorld();
        var child = SpawnFocusableNode(world);

        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child);

        var textWriter = world.Events.GetWriter<TextInputEvent>();
        textWriter.Write(new TextInputEvent { DeviceId = Guid.Empty, Text = "hello" });

        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        UIKeyboardRoutingSystem.PerformRouting(keyReader, textReader, focusState, world.Events);

        var uiTextReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;
        var events = uiTextReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
        Assert.Equal("hello", events[0].Text);
    }

    [Fact]
    public void Tab_NotRouted_ConsumedByFocusNavigation()
    {
        using var world = CreateWorld();
        var child = SpawnFocusableNode(world);

        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child);

        var keyWriter = world.Events.GetWriter<ButtonEvent<Key>>();
        keyWriter.Write(new ButtonEvent<Key> { DeviceId = Guid.Empty, Button = Key.Tab, State = ButtonState.JustPressed });

        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        UIKeyboardRoutingSystem.PerformRouting(keyReader, textReader, focusState, world.Events);

        var uiKeyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var events = uiKeyReader.Read();
        Assert.Equal(0, events.Length);
    }
}
