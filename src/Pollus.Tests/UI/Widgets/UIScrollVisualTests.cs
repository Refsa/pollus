using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIScrollVisualTests
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

    static Entity SpawnScrollContainer(World world, float contentHeight = 400f)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIScrollOffset(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Px(200), Length.Px(200)),
                    Overflow = new Point<Overflow>(Overflow.Scroll, Overflow.Scroll),
                }
            }
        )).Entity;

        // Add a tall child to create scrollable content
        var content = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Px(180), Length.Px(contentHeight)),
                }
            }
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, content);
        // First update: layout + visual system spawns overlay entities
        world.Update();
        // Second update: overlay entities materialized and positioned
        world.Update();
        return container;
    }

    [Fact]
    public void SpawnsVerticalThumbEntity_WhenContentOverflows()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.False(scroll.VerticalThumbEntity.IsNull, "VerticalThumbEntity should be spawned");
        Assert.True(world.Store.HasComponent<ComputedNode>(scroll.VerticalThumbEntity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(scroll.VerticalThumbEntity));
        Assert.True(world.Store.HasComponent<BorderRadius>(scroll.VerticalThumbEntity));
    }

    [Fact]
    public void VerticalThumb_IsChildOfContainer()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(world.Store.HasComponent<Child>(scroll.VerticalThumbEntity));
        var child = world.Store.GetComponent<Child>(scroll.VerticalThumbEntity);
        Assert.Equal(container, child.Parent);
    }

    [Fact]
    public void VerticalThumb_HasCorrectSize()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        var thumbComputed = world.Store.GetComponent<ComputedNode>(scroll.VerticalThumbEntity);

        // Thickness should be 6px
        Assert.Equal(6f, thumbComputed.Size.X, 0.1f);
        // Height: innerH=200, contentH=400, thumbH = max(20, (200/400)*200) = 100
        Assert.True(thumbComputed.Size.Y > 90f && thumbComputed.Size.Y < 110f,
            $"Expected thumb height ~100, got {thumbComputed.Size.Y}");
    }

    [Fact]
    public void VerticalThumb_PositionCounteractsScrollOffset()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        // Scroll down partway
        ref var scroll = ref world.Store.GetComponent<UIScrollOffset>(container);
        scroll.Offset.Y = 100f; // half of maxScroll (400 - 200 = 200)

        world.Update();

        scroll = world.Store.GetComponent<UIScrollOffset>(container);
        var thumbComputed = world.Store.GetComponent<ComputedNode>(scroll.VerticalThumbEntity);

        // The position should include scroll.Offset.Y to counteract the extract system's subtraction
        // scrollRatio = 100/200 = 0.5, trackH = 200 - 100 = 100, thumbY = 0 + 0.5*100 = 50
        // Counteract: position.Y = 50 + 100 (scroll offset) = 150
        Assert.True(thumbComputed.Position.Y > 140f,
            $"Expected thumb Y to include scroll offset counteraction, got {thumbComputed.Position.Y}");
    }

    [Fact]
    public void NoThumbSpawned_WhenContentFits()
    {
        using var world = CreateWorld();
        // Content is 100px, container is 200px — no overflow
        var container = SpawnScrollContainer(world, contentHeight: 100f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(scroll.VerticalThumbEntity.IsNull,
            "VerticalThumbEntity should not be spawned when content fits");
    }

    [Fact]
    public void VerticalThumb_AtTopWhenScrollIsZero()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        var thumbComputed = world.Store.GetComponent<ComputedNode>(scroll.VerticalThumbEntity);

        // scrollRatio = 0, so thumbY = borderTop + paddingTop + 0 = 0 (no border/padding)
        // Counteract: position.Y = 0 + 0 (scroll offset is 0) = 0
        Assert.True(thumbComputed.Position.Y < 1f,
            $"Expected thumb Y near 0 when scroll is 0, got {thumbComputed.Position.Y}");
    }
}
