using Pollus.Collections;
using Pollus.ECS;
using Pollus.Input;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Interaction;

public class UIHitTestTests
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
    public void MouseOverSingleNode_HoveredEntitySet()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // Child at (0,0) size 200x100 (flex row, first child)
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));

        Assert.Equal(child, hitResult.HoveredEntity);
    }

    [Fact]
    public void MouseOutsideAllNodes_HoveredEntityIsNull()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(500, 500));

        Assert.True(hitResult.HoveredEntity.IsNull);
    }

    [Fact]
    public void OverlappingSiblings_LastSiblingWins()
    {
        // Use absolute positioning so both siblings overlap at origin
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // First sibling: 200x200 at origin
        var sibling1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(200)),
                Position = Position.Absolute,
                Inset = new Rect<LengthPercentageAuto>(
                    LengthPercentageAuto.Px(0), LengthPercentageAuto.Auto,
                    LengthPercentageAuto.Px(0), LengthPercentageAuto.Auto
                ),
            }}
        )).Entity;

        // Second sibling: 200x200 at origin (overlaps first)
        var sibling2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(200)),
                Position = Position.Absolute,
                Inset = new Rect<LengthPercentageAuto>(
                    LengthPercentageAuto.Px(0), LengthPercentageAuto.Auto,
                    LengthPercentageAuto.Px(0), LengthPercentageAuto.Auto
                ),
            }}
        )).Entity;

        commands.AddChild(root, sibling1);
        commands.AddChild(root, sibling2);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));

        // Last sibling in DFS order (painter's order) wins
        Assert.Equal(sibling2, hitResult.HoveredEntity);
    }

    [Fact]
    public void NestedChild_ChildWinsOverParent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var parent = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(200)),
            }}
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, parent);
        commands.AddChild(parent, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));

        Assert.Equal(child, hitResult.HoveredEntity);
    }

    [Fact]
    public void DisabledNode_NotHittable()
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
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));

        Assert.True(hitResult.HoveredEntity.IsNull);
    }

    [Fact]
    public void NodeWithoutUIInteraction_Ignored()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // No UIInteraction component
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));

        Assert.True(hitResult.HoveredEntity.IsNull);
    }

    [Fact]
    public void ZeroSizeNode_Ignored()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(0), Dimension.Px(0)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(0, 0));

        Assert.True(hitResult.HoveredEntity.IsNull);
    }

    [Fact]
    public void FocusableEntities_CollectedInFocusOrder()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        var child3 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = false },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)) } }
        )).Entity;

        commands.AddChild(root, child1);
        commands.AddChild(root, child2);
        commands.AddChild(root, child3);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, Vec2f.Zero);

        Assert.Equal(2, focusState.FocusOrder.Count);
        Assert.Contains(child1, focusState.FocusOrder);
        Assert.Contains(child2, focusState.FocusOrder);
        Assert.DoesNotContain(child3, focusState.FocusOrder);
    }

    [Fact]
    public void PreviousHoveredEntity_TrackedCorrectly()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // First hit: hover child
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 50));
        Assert.Equal(child, hitResult.HoveredEntity);

        // Second hit: move outside
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(500, 500));
        Assert.True(hitResult.HoveredEntity.IsNull);
        Assert.Equal(child, hitResult.PreviousHoveredEntity);
    }

    [Fact]
    public void HitTest_WithTextChild_DoesNotCrash()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var button = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(50)),
            }}
        )).Entity;

        // UIText child — gets UINode+ComputedNode via Required chain
        var textChild = commands.Spawn(Entity.With(
            new UIText { Text = new NativeUtf8("Click me"), Size = 14f, Color = Color.WHITE },
            new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
            new UITextFont { Font = Handle<FontAsset>.Null },
            new UIStyle { Value = LayoutStyle.Default }
        )).Entity;

        commands.AddChild(root, button);
        commands.AddChild(button, textChild);
        world.Update();

        var hitResult = world.Resources.Get<UIHitTestResult>();
        var focusState = world.Resources.Get<UIFocusState>();
        var query = new Query(world);

        // Should not throw — text child is walked during DFS
        UIInteractionSystem.PerformHitTest(query, hitResult, focusState, new Vec2f(50, 25));

        Assert.Equal(button, hitResult.HoveredEntity);
    }

    [Fact]
    public void UIText_SpawnedEntity_HasComputedNode()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var textEntity = commands.Spawn(Entity.With(
            new UIText { Text = new NativeUtf8("Hello"), Size = 16f, Color = Color.WHITE },
            new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
            new UITextFont { Font = Handle<FontAsset>.Null },
            new UIStyle { Value = LayoutStyle.Default }
        )).Entity;

        commands.AddChild(root, textEntity);
        world.Update();

        // UIText requires UINode, which requires UIStyle + ComputedNode
        var query = new Query(world);
        Assert.True(query.Has<UINode>(textEntity), "UIText entity should have UINode via Required chain");
        Assert.True(query.Has<ComputedNode>(textEntity), "UIText entity should have ComputedNode via Required chain");
        Assert.True(query.Has<ContentSize>(textEntity), "UIText entity should have ContentSize via Required chain");
    }
}
