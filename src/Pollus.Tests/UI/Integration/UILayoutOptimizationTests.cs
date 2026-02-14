using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class UILayoutOptimizationTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Resources.Add(new CurrentDevice<Mouse>());
        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new ButtonInput<Key>());
        world.Prepare();
        return world;
    }

    [Fact]
    public void StableFrame_AdapterNotDirtyAfterLayout()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        ));

        // First update: layout computed, dirty cleared
        world.Update();
        // Second update: nothing changed
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        // After a stable frame where nothing changed, adapter should not be dirty
        Assert.False(adapter.IsDirty);
    }

    [Fact]
    public void StyleChange_SetsDirtyFlag()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        Assert.False(adapter.IsDirty);

        // Mutate a style — should trigger dirty
        ref var style = ref world.Store.GetComponent<UIStyle>(child);
        style.Value = LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(60)),
        };
        world.Update();

        // After the update with a style change, layout was recomputed and written back
        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(100f, computed.Size.X);
        Assert.Equal(60f, computed.Size.Y);
    }

    [Fact]
    public void HierarchyChange_TriggersRelayout()
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
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
            }}
        )).Entity;

        commands.AddChild(root, child1);
        world.Update();
        world.Update();

        // Add a new child — hierarchy change
        commands = world.GetCommands();
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(60), Dimension.Px(40)),
            }}
        )).Entity;
        commands.AddChild(root, child2);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child2);
        Assert.Equal(60f, computed.Size.X);
        Assert.Equal(40f, computed.Size.Y);
    }

    [Fact]
    public void ViewportResize_TriggersRelayout()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();
        world.Update();

        // Resize viewport
        ref var layoutRoot = ref world.Store.GetComponent<UILayoutRoot>(root);
        layoutRoot.Size = new Size<float>(400, 300);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(400f, computed.Size.X);
        Assert.Equal(300f, computed.Size.Y);
    }

    [Fact]
    public void StableFrame_SyncStatsShowNoWork()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        for (int i = 0; i < 10; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle { Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(30), Dimension.Px(20)),
                }}
            )).Entity;
            commands.AddChild(root, child);
        }

        world.Update();
        world.Update();
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        var stats = adapter.LastSyncStats;
        Assert.Equal(0, stats.StylesCopied);
        Assert.Equal(0, stats.NodesAdded);
        Assert.Equal(0, stats.NodesRemoved);
        Assert.Equal(0, stats.HierarchyRebuilds);
    }

    [Fact]
    public void StableFrame_HierarchyNotRebuilt()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        var stats = adapter.LastSyncStats;
        Assert.Equal(0, stats.HierarchyRebuilds);
    }
}
