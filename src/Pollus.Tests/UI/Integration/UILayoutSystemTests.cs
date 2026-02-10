using Pollus.ECS;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class UILayoutSystemTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    [Fact]
    public void RootNode_ComputedSizeMatchesViewport()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(root);
        Assert.Equal(800f, computed.Size.X);
        Assert.Equal(600f, computed.Size.Y);
    }

    [Fact]
    public void FixedSizeChild_ComputedNodeMatchesStyle()
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

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(100f, computed.Size.X);
        Assert.Equal(50f, computed.Size.Y);
        Assert.Equal(0f, computed.Position.X);
        Assert.Equal(0f, computed.Position.Y);
    }

    [Fact]
    public void FlexGrow_FillsMainAxis()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(400f, computed.Size.X);
    }

    [Fact]
    public void MultipleRoots_IndependentLayout()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root1 = commands.Spawn(Entity.With(
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
        commands.AddChild(root1, child1);

        var root2 = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(200, 100) }
        )).Entity;
        var child2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;
        commands.AddChild(root2, child2);

        world.Update();

        var c1 = world.Store.GetComponent<ComputedNode>(child1);
        Assert.Equal(100f, c1.Size.X);
        Assert.Equal(50f, c1.Size.Y);

        var c2 = world.Store.GetComponent<ComputedNode>(child2);
        Assert.Equal(200f, c2.Size.X);
    }

    [Fact]
    public void NoLayoutRoot_ChildNotComputed()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        // UINode without UILayoutRoot — root detected but skipped by ComputeLayout
        var root = commands.Spawn(Entity.With(new UINode())).Entity;
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        commands.AddChild(root, child);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(0f, computed.Size.X);
        Assert.Equal(0f, computed.Size.Y);
    }

    [Fact]
    public void DisplayNone_ComputedZeroed()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var visible = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;
        var hidden = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Display = Display.None,
                Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(50)),
            }}
        )).Entity;

        commands.AddChild(root, visible);
        commands.AddChild(root, hidden);
        world.Update();

        var visibleComputed = world.Store.GetComponent<ComputedNode>(visible);
        Assert.Equal(100f, visibleComputed.Size.X);

        var hiddenComputed = world.Store.GetComponent<ComputedNode>(hidden);
        Assert.Equal(0f, hiddenComputed.Size.X);
        Assert.Equal(0f, hiddenComputed.Size.Y);
    }

    [Fact]
    public void Padding_ShiftsChildAndWrittenToComputedNode()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<LengthPercentage>(
                    LengthPercentage.Px(10), LengthPercentage.Px(10),
                    LengthPercentage.Px(20), LengthPercentage.Px(20)
                ),
            }},
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

        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(10f, childComputed.Position.X);
        Assert.Equal(20f, childComputed.Position.Y);

        var rootComputed = world.Store.GetComponent<ComputedNode>(root);
        Assert.Equal(10f, rootComputed.PaddingLeft);
        Assert.Equal(10f, rootComputed.PaddingRight);
        Assert.Equal(20f, rootComputed.PaddingTop);
        Assert.Equal(20f, rootComputed.PaddingBottom);
    }

    [Fact]
    public void NestedHierarchy_PositionsCorrect()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
                Padding = new Rect<LengthPercentage>(
                    LengthPercentage.Px(10), LengthPercentage.Px(10),
                    LengthPercentage.Px(10), LengthPercentage.Px(10)
                ),
            }}
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Px(30)),
            }}
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, child);
        world.Update();

        var containerComputed = world.Store.GetComponent<ComputedNode>(container);
        Assert.Equal(200f, containerComputed.Size.X);
        Assert.Equal(0f, containerComputed.Position.X);

        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        Assert.Equal(50f, childComputed.Size.X);
        Assert.Equal(10f, childComputed.Position.X);
        Assert.Equal(10f, childComputed.Position.Y);
    }

    [Fact]
    public void UnroundedValues_PreservedInComputedNode()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(100, 100) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Px(33.33f), Dimension.Px(20)),
            }}
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(child);
        // Rounded size is integer
        Assert.Equal(MathF.Round(computed.Size.X), computed.Size.X);
        // Unrounded preserves sub-pixel value
        Assert.Equal(33.33f, computed.UnroundedSize.X, 0.01f);
    }

    [Fact]
    public void ViewportResize_RelayoutsCorrectly()
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

        Assert.Equal(800f, world.Store.GetComponent<ComputedNode>(child).Size.X);

        // Resize viewport
        ref var layoutRoot = ref world.Store.GetComponent<UILayoutRoot>(root);
        layoutRoot.Size = new Size<float>(400, 300);
        world.Update();

        Assert.Equal(400f, world.Store.GetComponent<ComputedNode>(child).Size.X);
    }
}
