using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Interaction;

public class UIFocusTests
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

    static (Entity child1, Entity child2, Entity child3) SpawnThreeFocusableNodes(World world)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(50)) } }
        )).Entity;

        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(50)) } }
        )).Entity;

        var child3 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, child1);
        commands.AddChild(root, child2);
        commands.AddChild(root, child3);
        world.Update();
        return (child1, child2, child3);
    }

    void BuildFocusOrder(World world)
    {
        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, Vec2f.Zero);
    }

    [Fact]
    public void ClickOnFocusable_SetsFocus_EmitsFocusEvent()
    {
        using var world = CreateWorld();
        var (child1, _, _) = SpawnThreeFocusableNodes(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Click on child1 (at 0,0 size 200x50)
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        Assert.Equal(child1, focusState.FocusedEntity);

        var focusReader = world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!;
        var events = focusReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(child1, events[0].Entity);
    }

    [Fact]
    public void Tab_CyclesForwardThroughFocusOrder()
    {
        using var world = CreateWorld();
        var (child1, child2, child3) = SpawnThreeFocusableNodes(world);

        BuildFocusOrder(world);
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // No focus initially, Tab should focus first
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(child1, focusState.FocusedEntity);

        // Consume events
        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();

        // Tab again → second
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(child2, focusState.FocusedEntity);

        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();

        // Tab again → third
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(child3, focusState.FocusedEntity);
    }

    [Fact]
    public void ShiftTab_CyclesBackward()
    {
        using var world = CreateWorld();
        var (child1, child2, child3) = SpawnThreeFocusableNodes(world);

        BuildFocusOrder(world);
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Focus the third item first
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child3);
        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();

        // Shift+Tab → second
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: false, shiftTabPressed: true, activatePressed: false);
        Assert.Equal(child2, focusState.FocusedEntity);

        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();

        // Shift+Tab → first
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: false, shiftTabPressed: true, activatePressed: false);
        Assert.Equal(child1, focusState.FocusedEntity);
    }

    [Fact]
    public void Tab_OnLastWrapsToFirst()
    {
        using var world = CreateWorld();
        var (child1, _, child3) = SpawnThreeFocusableNodes(world);

        BuildFocusOrder(world);
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Focus the last item
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child3);
        world.Events.GetReader<UIInteractionEvents.UIFocusEvent>()!.Read();

        // Tab should wrap to first
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: true, shiftTabPressed: false, activatePressed: false);
        Assert.Equal(child1, focusState.FocusedEntity);
    }

    [Fact]
    public void Enter_OnFocused_EmitsClickEvent()
    {
        using var world = CreateWorld();
        var (child1, _, _) = SpawnThreeFocusableNodes(world);

        BuildFocusOrder(world);
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.SetFocus(query, focusState, world.Events, child1);
        world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!.Read();

        // Enter activates focused entity
        UIInteractionSystem.PerformFocusNavigation(query, focusState, world.Events, tabPressed: false, shiftTabPressed: false, activatePressed: true);

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var clicks = clickReader.Read();
        Assert.Equal(1, clicks.Length);
        Assert.Equal(child1, clicks[0].Entity);
    }

    [Fact]
    public void ClickEmpty_BlursCurrentFocus()
    {
        using var world = CreateWorld();
        var (child1, _, _) = SpawnThreeFocusableNodes(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Focus child1
        UIInteractionSystem.SetFocus(query, focusState, world.Events, child1);
        world.Events.GetReader<UIInteractionEvents.UIBlurEvent>()!.Read();

        // Click empty space
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(500, 500));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        Assert.True(focusState.FocusedEntity.IsNull);

        var blurReader = world.Events.GetReader<UIInteractionEvents.UIBlurEvent>()!;
        var blurs = blurReader.Read();
        Assert.Equal(1, blurs.Length);
        Assert.Equal(child1, blurs[0].Entity);
    }
}
