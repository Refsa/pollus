using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class UIInteractionIntegrationTests
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
    public void FullInteractionFlow_HoverPressReleaseClick()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(200), Length.Px(80)),
            }}
        )).Entity;

        commands.AddChild(root, button);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Step 1: Hover
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(100, 40));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        var interaction = world.Store.GetComponent<UIInteraction>(button);
        Assert.True(interaction.IsHovered, "Should be hovered");
        Assert.False(interaction.IsPressed, "Should not be pressed yet");

        var hoverEnterEvents = world.Events.GetReader<UIInteractionEvents.UIHoverEnterEvent>()!.Read();
        Assert.Equal(1, hoverEnterEvents.Length);

        // Step 2: Press
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(100, 40));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        interaction = world.Store.GetComponent<UIInteraction>(button);
        Assert.True(interaction.IsPressed, "Should be pressed");
        Assert.True(interaction.IsFocused, "Should be focused on press");

        var pressEvents = world.Events.GetReader<UIInteractionEvents.UIPressEvent>()!.Read();
        Assert.Equal(1, pressEvents.Length);

        // Step 3: Release → Click
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(100, 40));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: true);

        interaction = world.Store.GetComponent<UIInteraction>(button);
        Assert.False(interaction.IsPressed, "Should no longer be pressed");

        var clickEvents = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!.Read();
        Assert.Equal(1, clickEvents.Length);
        Assert.Equal(button, clickEvents[0].Entity);
    }

    [Fact]
    public void MultipleButtons_IndependentInteraction()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var btn1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(200), Length.Px(50)),
            }}
        )).Entity;

        var btn2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(200), Length.Px(50)),
            }}
        )).Entity;

        commands.AddChild(root, btn1);
        commands.AddChild(root, btn2);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        var enterReader = world.Events.GetReader<UIInteractionEvents.UIHoverEnterEvent>()!;
        var exitReader = world.Events.GetReader<UIInteractionEvents.UIHoverExitEvent>()!;

        // Hover btn1 (at y=0..50)
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        Assert.True(world.Store.GetComponent<UIInteraction>(btn1).IsHovered);
        Assert.False(world.Store.GetComponent<UIInteraction>(btn2).IsHovered);

        // Consume events from first step
        enterReader.Read();
        exitReader.Read();

        // Move to btn2 (at y=50..100)
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 75));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false);

        Assert.False(world.Store.GetComponent<UIInteraction>(btn1).IsHovered);
        Assert.True(world.Store.GetComponent<UIInteraction>(btn2).IsHovered);

        var exitEvents = exitReader.Read();
        Assert.Equal(1, exitEvents.Length);
        Assert.Equal(btn1, exitEvents[0].Entity);

        var enterEvents = enterReader.Read();
        Assert.Equal(1, enterEvents.Length);
        Assert.Equal(btn2, enterEvents[0].Entity);
    }

    [Fact]
    public void FocusNavigation_TabCyclesThroughButtons()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var btn1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(50)) } }
        )).Entity;

        var btn2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(50)) } }
        )).Entity;

        commands.AddChild(root, btn1);
        commands.AddChild(root, btn2);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Build focus order
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, Vec2f.Zero);

        // Tab → btn1
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(btn1, focusState.FocusedEntity);
        Assert.True(world.Store.GetComponent<UIInteraction>(btn1).IsFocused);

        // Tab → btn2
        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(btn2, focusState.FocusedEntity);
        Assert.True(world.Store.GetComponent<UIInteraction>(btn2).IsFocused);
        Assert.False(world.Store.GetComponent<UIInteraction>(btn1).IsFocused);

        // Tab → wraps to btn1
        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(btn1, focusState.FocusedEntity);
    }
}
