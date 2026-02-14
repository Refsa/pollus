using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Interaction;

public class UIPointerCaptureTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    static Entity SpawnInteractiveNode(World world)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
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
    public void Press_CapturesEntity()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        Assert.Equal(child, hitResult.CapturedEntity);
    }

    [Fact]
    public void Release_ClearsCapture()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Press
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);
        Assert.Equal(child, hitResult.CapturedEntity);

        // Release
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: true);

        Assert.True(hitResult.CapturedEntity.IsNull);
    }

    [Fact]
    public void Move_WhileCaptured_SendsDragEvent()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Press at (50, 25)
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        // Move to (70, 30) — simulate pointer move while captured
        hitResult.PreviousMousePosition = hitResult.MousePosition;
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(70, 30));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: false);

        var dragReader = world.Events.GetReader<UIInteractionEvents.UIDragEvent>()!;
        var drags = dragReader.Read();
        Assert.Equal(1, drags.Length);
        Assert.Equal(child, drags[0].Entity);
        Assert.Equal(70f, drags[0].PositionX, 0.01f);
        Assert.Equal(30f, drags[0].PositionY, 0.01f);
        Assert.Equal(20f, drags[0].DeltaX, 0.01f);
        Assert.Equal(5f, drags[0].DeltaY, 0.01f);
    }

    [Fact]
    public void DragEvent_IncludesCorrectDelta()
    {
        using var world = CreateWorld();
        var child = SpawnInteractiveNode(world);

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Press at (100, 100)
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(100, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: true);

        // Move to (150, 25)
        hitResult.PreviousMousePosition = hitResult.MousePosition;
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(150, 25));
        UIInteractionSystem.PerformUpdateState(query, hitResult, focusState, world.Events, mouseDown: false, mouseUp: false);

        var dragReader = world.Events.GetReader<UIInteractionEvents.UIDragEvent>()!;
        var drags = dragReader.Read();
        Assert.Equal(1, drags.Length);
        Assert.Equal(50f, drags[0].DeltaX, 0.01f);
        Assert.Equal(0f, drags[0].DeltaY, 0.01f);
    }
}
