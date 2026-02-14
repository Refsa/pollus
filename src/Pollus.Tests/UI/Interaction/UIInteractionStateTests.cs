using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Interaction;

public class UIInteractionStateTests
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

    static Entity SpawnInteractiveNode(World world, float w = 200, float h = 100)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(w), Length.Px(h)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update(); // run layout
        return child;
    }

    [Fact]
    public void HoverEnter_SetsHoveredFlag_EmitsEvent()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Simulate mouse over child
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        var interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.True(interaction.IsHovered);

        var reader = world.Events.GetReader<UIInteractionEvents.UIHoverEnterEvent>()!;
        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
    }

    [Fact]
    public void HoverExit_ClearsFlag_EmitsEvent()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Hover enter
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        // Consume events from first frame
        world.Events.GetReader<UIInteractionEvents.UIHoverEnterEvent>()!.Read();
        world.Events.GetReader<UIInteractionEvents.UIHoverExitEvent>()!.Read();

        // Hover exit
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(500, 500));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        var interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.False(interaction.IsHovered);

        var reader = world.Events.GetReader<UIInteractionEvents.UIHoverExitEvent>()!;
        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
    }

    [Fact]
    public void PressAndReleaseSameEntity_EmitsClickEvent()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Hover + press
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        var interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.True(interaction.IsPressed);

        // Consume press events
        world.Events.GetReader<UIInteractionEvents.UIPressEvent>()!.Read();
        world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!.Read();
        world.Events.GetReader<UIInteractionEvents.UIReleaseEvent>()!.Read();

        // Release on same entity
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: true);

        interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.False(interaction.IsPressed);

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var clicks = clickReader.Read();
        Assert.Equal(1, clicks.Length);
        Assert.Equal(child, clicks[0].Entity);

        var releaseReader = world.Events.GetReader<UIInteractionEvents.UIReleaseEvent>()!;
        var releases = releaseReader.Read();
        Assert.Equal(1, releases.Length);
    }

    [Fact]
    public void PressAndReleaseOutside_NoClickEvent()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Hover + press
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        // Consume events
        world.Events.GetReader<UIInteractionEvents.UIPressEvent>()!.Read();
        world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!.Read();
        world.Events.GetReader<UIInteractionEvents.UIReleaseEvent>()!.Read();

        // Move outside and release
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(500, 500));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: true);

        var releaseReader = world.Events.GetReader<UIInteractionEvents.UIReleaseEvent>()!;
        var releases = releaseReader.Read();
        Assert.Equal(1, releases.Length);

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var clicks = clickReader.Read();
        Assert.Equal(0, clicks.Length);
    }

    [Fact]
    public void DisabledEntity_NoStateTransitions()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { State = InteractionState.Disabled },
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(200), Length.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        var interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.True(interaction.IsDisabled);
        Assert.False(interaction.IsHovered);
        Assert.False(interaction.IsPressed);
    }

    [Fact]
    public void PressSetsEntityFocus()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        Assert.Equal(child, focusState.FocusedEntity);

        var interaction = world.Store.GetComponent<UIInteraction>(child);
        Assert.True(interaction.IsFocused);

        var focusReader = world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!;
        var events = focusReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child, events[0].Entity);
    }
}
