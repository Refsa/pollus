using Pollus.ECS;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class UILifecycleTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    [Fact]
    public void StyleChange_TriggersRelayout()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        commands.AddChild(root, child);
        world.Update();

        Assert.Equal(100f, world.Store.GetComponent<ComputedNode>(child).Size.X);

        // Modify style directly
        ref var style = ref world.Store.GetComponent<UIStyle>(child);
        style.Value = LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(80)),
        };
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(200f, computed.Size.X);
        Assert.Equal(80f, computed.Size.Y);
    }

    [Fact]
    public void AddChild_AfterInitialLayout()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;
        var child1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        commands.AddChild(root, child1);
        world.Update();

        Assert.Equal(100f, world.Store.GetComponent<ComputedNode>(child1).Size.X);

        // Add another child in next frame
        commands = world.GetCommands();
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(80), Dimension.Px(40)),
            }}
        )).Entity;
        commands.AddChild(root, child2);
        world.Update();

        var c1 = world.Store.GetComponent<ComputedNode>(child1);
        Assert.Equal(100f, c1.Size.X);
        Assert.Equal(0f, c1.Position.X);

        var c2 = world.Store.GetComponent<ComputedNode>(child2);
        Assert.Equal(80f, c2.Size.X);
        Assert.Equal(100f, c2.Position.X); // follows child1 in row direction
    }

    [Fact]
    public void MultipleUpdates_StableResults()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        commands.AddChild(root, child);

        world.Update();
        var first = world.Store.GetComponent<ComputedNode>(child);

        world.Update();
        var second = world.Store.GetComponent<ComputedNode>(child);

        world.Update();
        var third = world.Store.GetComponent<ComputedNode>(child);

        Assert.Equal(first.Size.X, second.Size.X);
        Assert.Equal(first.Size.Y, second.Size.Y);
        Assert.Equal(first.Position.X, second.Position.X);
        Assert.Equal(second.Size.X, third.Size.X);
    }

    [Fact]
    public void StyleChange_AfterAddedFlagExpires_StillRelayouts()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        commands.AddChild(root, child);

        // Burn through frames until Added flag expires (2-tick window)
        world.Update();
        world.Update();
        world.Update();
        world.Update();
        Assert.Equal(100f, world.Store.GetComponent<ComputedNode>(child).Size.X);

        // Modify style via ref mutation (does NOT set ECS Changed flag)
        ref var style = ref world.Store.GetComponent<UIStyle>(child);
        style.Value = LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(80)),
        };
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(200f, computed.Size.X);
        Assert.Equal(80f, computed.Size.Y);
    }

    [Fact]
    public void EmptyTree_NoCrash()
    {
        using var world = CreateWorld();
        world.Update();
        world.Update();
        // No entities — should not crash
    }

    [Fact]
    public void DespawnChild_RelayoutsAfterHierarchyCleanup()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;
        var child1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(80), Dimension.Px(40)),
            }}
        )).Entity;
        commands.AddChild(root, child1);
        commands.AddChild(root, child2);
        world.Update();

        Assert.Equal(100f, world.Store.GetComponent<ComputedNode>(child2).Position.X);

        // Remove child1 from hierarchy (fixes linked list), then despawn.
        // RemoveChild (priority 19) executes before Despawn (priority 0).
        commands = world.GetCommands();
        commands.RemoveChild(root, child1);
        commands.Despawn(child1);
        world.Update();

        var c2 = world.Store.GetComponent<ComputedNode>(child2);
        Assert.Equal(0f, c2.Position.X); // now first child in row
    }

    [Fact]
    public void DisplayNoneParent_ChildrenExcluded()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;
        var hiddenParent = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Display = Display.None,
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            }}
        )).Entity;
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
            }}
        )).Entity;
        commands.AddChild(root, hiddenParent);
        commands.AddChild(hiddenParent, child);
        world.Update();

        var parentComputed = world.Store.GetComponent<ComputedNode>(hiddenParent);
        Assert.Equal(0f, parentComputed.Size.X);

        // Child of Display.None parent gets zero layout too
        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(0f, childComputed.Size.X);
        Assert.Equal(0f, childComputed.Size.Y);
    }
}
