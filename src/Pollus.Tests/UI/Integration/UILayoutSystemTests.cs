using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Mathematics;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Integration;

public class UILayoutSystemTests
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
                Size = new Size<Length>(Length.Px(100), Length.Px(50)),
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
                Size = new Size<Length>(Length.Px(100), Length.Px(50)),
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
                Size = new Size<Length>(Length.Px(100), Length.Px(50)),
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
                Size = new Size<Length>(Length.Px(100), Length.Px(50)),
            }}
        )).Entity;
        var hidden = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Display = Display.None,
                Size = new Size<Length>(Length.Px(100), Length.Px(50)),
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
                Padding = new Rect<Length>(
                    Length.Px(10), Length.Px(10),
                    Length.Px(20), Length.Px(20)
                ),
            }},
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(50), Length.Px(30)),
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
    public void PaddedRoot_ChildFillsInnerArea_NoOverflow()
    {
        // A flex-grow child in a padded root should fill exactly the
        // content area: its right/bottom edge should stop at
        // (parent.Size - parent.PaddingRight/Bottom).
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                FlexDirection = FlexDirection.Column,
                Padding = new Rect<Length>(
                    Length.Px(16), Length.Px(16),
                    Length.Px(16), Length.Px(16)),
            }},
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var rootC = world.Store.GetComponent<ComputedNode>(root);
        var childC = world.Store.GetComponent<ComputedNode>(child);

        // Child position already includes parent padding (flex convention)
        Assert.Equal(16f, childC.Position.X);
        Assert.Equal(16f, childC.Position.Y);

        // Child fills exactly the inner area — no overflow
        float childRightEdge = childC.Position.X + childC.Size.X;
        float childBottomEdge = childC.Position.Y + childC.Size.Y;
        Assert.Equal(rootC.Size.X - 16f, childRightEdge);
        Assert.Equal(rootC.Size.Y - 16f, childBottomEdge);

        // Absolute screen position = parentPos + childPos (no contentOffset)
        // because the flex layout already includes parent padding in Position.
        Vec2f rootAbsPos = new(rootC.Position.X, rootC.Position.Y);
        Vec2f childAbsScreen = rootAbsPos + new Vec2f(childC.Position.X, childC.Position.Y);
        Assert.Equal(16f, childAbsScreen.X);
        Assert.Equal(16f, childAbsScreen.Y);
    }

    [Fact]
    public void NestedPadding_AbsolutePositionAccountsForAllLevels()
    {
        // For nested containers, each child's Position includes its
        // direct parent's padding.  Absolute screen position is computed
        // by summing positions up the tree — no extra contentOffset.
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<Length>(
                    Length.Px(10), Length.Px(10),
                    Length.Px(10), Length.Px(10)),
            }},
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(200), Length.Px(100)),
                Padding = new Rect<Length>(
                    Length.Px(8), Length.Px(8),
                    Length.Px(8), Length.Px(8)),
            }}
        )).Entity;

        var leaf = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(50), Length.Px(30)),
            }}
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, leaf);
        world.Update();

        var rootC = world.Store.GetComponent<ComputedNode>(root);
        var containerC = world.Store.GetComponent<ComputedNode>(container);
        var leafC = world.Store.GetComponent<ComputedNode>(leaf);

        // Container is offset by root's padding
        Assert.Equal(10f, containerC.Position.X);
        Assert.Equal(10f, containerC.Position.Y);

        // Leaf is offset by container's padding
        Assert.Equal(8f, leafC.Position.X);
        Assert.Equal(8f, leafC.Position.Y);

        // Absolute screen position = sum of all Position values up the tree
        float leafAbsX = rootC.Position.X + containerC.Position.X + leafC.Position.X;
        float leafAbsY = rootC.Position.Y + containerC.Position.Y + leafC.Position.Y;
        Assert.Equal(18f, leafAbsX);  // 0 + 10 + 8
        Assert.Equal(18f, leafAbsY);  // 0 + 10 + 8
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
                Size = new Size<Length>(Length.Px(200), Length.Px(100)),
                Padding = new Rect<Length>(
                    Length.Px(10), Length.Px(10),
                    Length.Px(10), Length.Px(10)
                ),
            }}
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(50), Length.Px(30)),
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
                Size = new Size<Length>(Length.Px(33.33f), Length.Px(20)),
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

    [Fact]
    public void RootWithPadding_OuterSizeEqualsViewport()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<Length>(
                    Length.Px(16), Length.Px(16),
                    Length.Px(16), Length.Px(16)),
            }},
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(root);
        Assert.Equal(800f, computed.Size.X);
        Assert.Equal(600f, computed.Size.Y);
    }

    [Fact]
    public void RootWithPadding_FlexGrowChildFillsInnerSpace()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<Length>(
                    Length.Px(20), Length.Px(20),
                    Length.Px(10), Length.Px(10)),
            }},
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        // Inner width = 400 - 20 - 20 = 360
        Assert.Equal(360f, childComputed.Size.X);
        // Inner height = 300 - 10 - 10 = 280
        Assert.Equal(280f, childComputed.Size.Y);
    }

    [Fact]
    public void RootWithPaddingAndBorder_OuterSizeEqualsViewport()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<Length>(
                    Length.Px(10), Length.Px(10),
                    Length.Px(10), Length.Px(10)),
                Border = new Rect<Length>(
                    Length.Px(2), Length.Px(2),
                    Length.Px(2), Length.Px(2)),
            }},
            new UILayoutRoot { Size = new Size<float>(500, 400) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var rootComputed = world.Store.GetComponent<ComputedNode>(root);
        Assert.Equal(500f, rootComputed.Size.X);
        Assert.Equal(400f, rootComputed.Size.Y);

        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        // Inner = 500 - 2*10 - 2*2 = 476
        Assert.Equal(476f, childComputed.Size.X);
        // Inner = 400 - 2*10 - 2*2 = 376
        Assert.Equal(376f, childComputed.Size.Y);
    }

    [Fact]
    public void RootContentBox_PaddingAddsToOuterSize()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                BoxSizing = BoxSizing.ContentBox,
                Padding = new Rect<Length>(
                    Length.Px(20), Length.Px(20),
                    Length.Px(20), Length.Px(20)),
            }},
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        var rootComputed = world.Store.GetComponent<ComputedNode>(root);
        // Content-box: outer = viewport + padding = 400+40, 300+40
        Assert.Equal(440f, rootComputed.Size.X);
        Assert.Equal(340f, rootComputed.Size.Y);

        var childComputed = world.Store.GetComponent<ComputedNode>(child);
        // Inner content area = full viewport dimensions
        Assert.Equal(400f, childComputed.Size.X);
        Assert.Equal(300f, childComputed.Size.Y);
    }

    [Fact]
    public void ViewportResize_WithPaddedRoot_RelayoutsCorrectly()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Padding = new Rect<Length>(
                    Length.Px(16), Length.Px(16),
                    Length.Px(16), Length.Px(16)),
            }},
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } }
        )).Entity;

        commands.AddChild(root, child);
        world.Update();

        Assert.Equal(800f, world.Store.GetComponent<ComputedNode>(root).Size.X);
        Assert.Equal(768f, world.Store.GetComponent<ComputedNode>(child).Size.X);

        // Resize viewport
        ref var layoutRoot = ref world.Store.GetComponent<UILayoutRoot>(root);
        layoutRoot.Size = new Size<float>(400, 300);
        world.Update();

        Assert.Equal(400f, world.Store.GetComponent<ComputedNode>(root).Size.X);
        Assert.Equal(368f, world.Store.GetComponent<ComputedNode>(child).Size.X);
    }

    [Fact]
    public void ContentSize_LeafUsesIntrinsicMeasurement()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();
        var adapter = world.Resources.Get<UITreeAdapter>();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                AlignItems = AlignItems.FlexStart,
            }},
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var leaf = commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize()
        )).Entity;

        commands.AddChild(root, leaf);

        adapter.SetMeasureFunc(leaf, (known, avail) =>
            new Size<float>(120, 30));

        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(leaf);
        Assert.Equal(120f, computed.Size.X);
        Assert.Equal(30f, computed.Size.Y);
    }

    [Fact]
    public void ContentSize_WithFlexGrow_ExpandsBeyondIntrinsic()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();
        var adapter = world.Resources.Get<UITreeAdapter>();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                FlexDirection = FlexDirection.Row,
                AlignItems = AlignItems.FlexStart,
            }},
            new UILayoutRoot { Size = new Size<float>(400, 200) }
        )).Entity;

        var measured = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexGrow = 1f } },
            new ContentSize()
        )).Entity;

        var fixedChild = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(100), Length.Auto),
            }}
        )).Entity;

        commands.AddChild(root, measured);
        commands.AddChild(root, fixedChild);

        adapter.SetMeasureFunc(measured, (known, avail) =>
            new Size<float>(50, 25));

        world.Update();

        var measuredComputed = world.Store.GetComponent<ComputedNode>(measured);
        // Intrinsic 50, fixed 100, remaining 250 → measured gets 300
        Assert.Equal(300f, measuredComputed.Size.X);
    }

    [Fact]
    public void ContentSize_TextWrapping_HeightDependsOnWidth()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();
        var adapter = world.Resources.Get<UITreeAdapter>();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = LayoutStyle.Default with { FlexDirection = FlexDirection.Column } },
            new UILayoutRoot { Size = new Size<float>(200, 400) }
        )).Entity;

        var text = commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize()
        )).Entity;

        commands.AddChild(root, text);

        // Simulate 500px of text content at 20px line height
        adapter.SetMeasureFunc(text, (known, avail) =>
        {
            float w = known.Width ?? avail.Width.UnwrapOr(float.MaxValue);
            float lines = MathF.Ceiling(500f / w);
            return new Size<float>(MathF.Min(500f, w), lines * 20f);
        });

        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(text);
        Assert.Equal(200f, computed.Size.X);
        Assert.Equal(60f, computed.Size.Y); // 500/200 = 3 lines * 20 = 60
    }

    [Fact]
    public void ContentSize_WithoutMeasureFunc_SizesAsEmpty()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(400, 300) }
        )).Entity;

        var leaf = commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize()
        )).Entity;

        commands.AddChild(root, leaf);
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(leaf);
        Assert.Equal(0f, computed.Size.X);
    }
}
