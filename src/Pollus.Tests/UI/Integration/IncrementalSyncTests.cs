using Pollus.ECS;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class IncrementalSyncTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UISystemsPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    [Fact]
    public void StableFrame_ZeroStylesCopied()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        for (int i = 0; i < 20; i++)
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

        // Initial layout + stabilize
        world.Update();
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();

        world.Update();

        Assert.Equal(0, adapter.LastSyncStats.StylesCopied);
        Assert.Equal(0, adapter.LastSyncStats.NodesAdded);
        Assert.Equal(0, adapter.LastSyncStats.NodesRemoved);
    }

    [Fact]
    public void SingleStyleChange_OnlyOneStyleCopied()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var children = new Entity[50];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle { Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(10), Dimension.Px(10)),
                }}
            )).Entity;
            commands.AddChild(root, children[i]);
        }

        world.Update();
        world.Update();

        ref var style = ref world.Store.GetComponent<UIStyle>(children[25]);
        style.Value = LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(20), Dimension.Px(20)),
        };
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        Assert.Equal(1, adapter.LastSyncStats.StylesCopied);

        Assert.Equal(20f, world.Store.GetComponent<ComputedNode>(children[25]).Size.X);
    }

    [Fact]
    public void AddNewChild_TrackedAsNodeAdded()
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

        commands = world.GetCommands();
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(40), Dimension.Px(25)),
            }}
        )).Entity;
        commands.AddChild(root, child2);
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        Assert.Equal(1, adapter.LastSyncStats.NodesAdded);
        Assert.Equal(0, adapter.LastSyncStats.NodesRemoved);

        Assert.Equal(40f, world.Store.GetComponent<ComputedNode>(child2).Size.X);
    }

    [Fact]
    public void DespawnEntity_TrackedAsNodeRemoved()
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
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(40), Dimension.Px(25)),
            }}
        )).Entity;
        commands.AddChild(root, child1);
        commands.AddChild(root, child2);

        world.Update();
        world.Update();

        commands = world.GetCommands();
        commands.RemoveChild(root, child1);
        commands.Despawn(child1);
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        Assert.Equal(1, adapter.LastSyncStats.NodesRemoved);

        Assert.Equal(0f, world.Store.GetComponent<ComputedNode>(child2).Position.X);
    }

    [Fact]
    public void NodeRemoval_FreesSlotAndMaintainsCapacity()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;
        var children = new Entity[3];
        for (int i = 0; i < 3; i++)
        {
            children[i] = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle { Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
                }}
            )).Entity;
            commands.AddChild(root, children[i]);
        }
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        int nodeCountBefore = adapter.NodeCapacity;
        Assert.Equal(4, adapter.ActiveNodeCount);

        commands = world.GetCommands();
        commands.RemoveChild(root, children[1]);
        commands.Despawn(children[1]);
        world.Update();

        // NodeCapacity stays the same (slot is freed, not shrunk)
        Assert.Equal(nodeCountBefore, adapter.NodeCapacity);
        Assert.Equal(3, adapter.ActiveNodeCount);

        Assert.Equal(50f, world.Store.GetComponent<ComputedNode>(children[0]).Size.X);
        Assert.Equal(50f, world.Store.GetComponent<ComputedNode>(children[2]).Size.X);
        // children[2] should now be at position 50 (right after children[0])
        Assert.Equal(50f, world.Store.GetComponent<ComputedNode>(children[2]).Position.X);
    }
}
