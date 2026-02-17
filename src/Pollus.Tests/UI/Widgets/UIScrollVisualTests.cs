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
        world.Resources.Add(new CurrentDevice<Keyboard>());
        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new ButtonInput<Key>());
        world.Prepare();
        return world;
    }

    static Entity SpawnHorizontalScrollContainer(World world, float contentWidth = 400f)
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

        // Add a wide child to create horizontally scrollable content
        var content = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Px(contentWidth), Length.Px(180)),
                }
            }
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, content);
        world.Update();
        world.Update();
        return container;
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

    [Fact]
    public void ScrollWheelInput_ChangesVerticalOffset()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        // Verify ContentSize.Y > Size.Y (overflow exists)
        var computed = world.Store.GetComponent<ComputedNode>(container);
        Assert.True(computed.ContentSize.Y > computed.Size.Y,
            $"ContentSize.Y ({computed.ContentSize.Y}) should exceed Size.Y ({computed.Size.Y})");

        // Set up mouse device with scroll wheel input
        var mouse = new Mouse(0);
        mouse.SetPosition(100, 100); // within container bounds (container at 0,0, size 200x200)
        mouse.SetAxisState(MouseAxis.ScrollY, -1f); // scroll down

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;

        // Run the world update (will trigger hit test + scroll system)
        world.Update();

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(scroll.Offset.Y > 0f,
            $"Expected Offset.Y > 0 after scrolling down, got {scroll.Offset.Y}");
    }

    [Fact]
    public void ScrollWheelInput_ClampsToMaxScroll()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        // Manually set scroll offset beyond max
        ref var scroll = ref world.Store.GetComponent<UIScrollOffset>(container);
        scroll.Offset.Y = 999f;

        // Provide mouse input to trigger clamp
        var mouse = new Mouse(0);
        mouse.SetPosition(100, 100);
        mouse.SetAxisState(MouseAxis.ScrollY, -0.001f); // tiny scroll down

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;
        world.Update();

        scroll = world.Store.GetComponent<UIScrollOffset>(container);
        var computed = world.Store.GetComponent<ComputedNode>(container);
        var innerHeight = computed.Size.Y - computed.PaddingTop - computed.PaddingBottom
                          - computed.BorderTop - computed.BorderBottom;
        var maxScroll = computed.ContentSize.Y - innerHeight;

        Assert.True(scroll.Offset.Y <= maxScroll + 0.01f,
            $"Offset.Y ({scroll.Offset.Y}) should be clamped to maxScroll ({maxScroll})");
    }

    [Fact]
    public void ScrollWheelInput_ChangesHorizontalOffset()
    {
        using var world = CreateWorld();
        var container = SpawnHorizontalScrollContainer(world, contentWidth: 400f);

        var computed = world.Store.GetComponent<ComputedNode>(container);
        Assert.True(computed.ContentSize.X > computed.Size.X,
            $"ContentSize.X ({computed.ContentSize.X}) should exceed Size.X ({computed.Size.X})");

        var mouse = new Mouse(0);
        mouse.SetPosition(100, 100);
        mouse.SetAxisState(MouseAxis.ScrollX, -1f); // scroll right

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;
        world.Update();

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(scroll.Offset.X > 0f,
            $"Expected Offset.X > 0 after scrolling right, got {scroll.Offset.X}");
    }

    [Fact]
    public void ScrollWheelInput_ClampsHorizontalToMaxScroll()
    {
        using var world = CreateWorld();
        var container = SpawnHorizontalScrollContainer(world, contentWidth: 400f);

        ref var scroll = ref world.Store.GetComponent<UIScrollOffset>(container);
        scroll.Offset.X = 999f;

        var mouse = new Mouse(0);
        mouse.SetPosition(100, 100);
        mouse.SetAxisState(MouseAxis.ScrollX, -0.001f);

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;
        world.Update();

        scroll = world.Store.GetComponent<UIScrollOffset>(container);
        var computed = world.Store.GetComponent<ComputedNode>(container);
        var innerWidth = computed.Size.X - computed.PaddingLeft - computed.PaddingRight
                         - computed.BorderLeft - computed.BorderRight;
        var maxScroll = computed.ContentSize.X - innerWidth;

        Assert.True(scroll.Offset.X <= maxScroll + 0.01f,
            $"Offset.X ({scroll.Offset.X}) should be clamped to maxScroll ({maxScroll})");
    }

    [Fact]
    public void ThumbEntities_HaveUIInteraction()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(world.Store.HasComponent<UIInteraction>(scroll.VerticalThumbEntity),
            "VerticalThumbEntity should have UIInteraction for drag support");
    }

    [Fact]
    public void ShiftScroll_MapsVerticalToHorizontal()
    {
        using var world = CreateWorld();
        // Container with VERTICAL overflow (400px content in 200px container)
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        // Sanity: verify vertical scroll works without shift
        var mouse1 = new Mouse(0);
        mouse1.SetPosition(100, 100);
        mouse1.SetAxisState(MouseAxis.ScrollY, -1f);
        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse1;
        world.Update();

        var sanityScroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(sanityScroll.Offset.Y > 0f,
            $"Sanity: vertical scroll should work, got Offset.Y={sanityScroll.Offset.Y}");

        // Reset offset
        ref var resetScroll = ref world.Store.GetComponent<UIScrollOffset>(container);
        resetScroll.Offset = Vec2f.Zero;

        // Now test shift+scroll: vertical scroll should be redirected to horizontal
        var mouse2 = new Mouse(0);
        mouse2.SetPosition(100, 100);
        mouse2.SetAxisState(MouseAxis.ScrollY, -1f); // vertical scroll

        // Must use the InputManager's keyboard (UpdateCurrentDevice overwrites CurrentDevice<Keyboard>)
        var keyboard = world.Resources.Get<InputManager>().GetDevice<Keyboard>("keyboard")!;
        keyboard.SetKeyState(Key.LeftShift, true);

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse2;

        world.Update();

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        // If shift works: scrollY gets swapped to scrollX, so Offset.Y stays 0
        // (and Offset.X = 0 too since no horizontal overflow, but that's OK)
        Assert.True(scroll.Offset.Y == 0f,
            $"With shift held, vertical scroll should become horizontal, but got Offset.Y={scroll.Offset.Y}");
    }

    [Fact]
    public void VerticalThumbDrag_UpdatesScrollOffset()
    {
        using var world = CreateWorld();
        var container = SpawnScrollContainer(world, contentHeight: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.False(scroll.VerticalThumbEntity.IsNull);

        // Get thumb position for hit testing
        var thumbComputed = world.Store.GetComponent<ComputedNode>(scroll.VerticalThumbEntity);
        // Thumb is at right edge of container: X ~192, Y ~0, Size: 6x100
        // Container is at (0,0), so thumb absolute position is ~(192, 0)

        var mouse = new Mouse(0);
        // Position mouse over thumb
        mouse.SetPosition((int)(thumbComputed.Position.X + 3), (int)(thumbComputed.Position.Y + 10));
        mouse.SetButtonState(MouseButton.Left, true); // JustPressed

        var mouseInput = world.Resources.Get<ButtonInput<MouseButton>>();
        mouseInput.AddDevice(mouse.Id, mouse);
        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;

        // Frame 1: mouse press on thumb → entity gets captured
        world.Update();

        // Frame 2: move mouse down → drag event generated
        mouse.SetPosition((int)(thumbComputed.Position.X + 3), (int)(thumbComputed.Position.Y + 50));
        mouse.SetButtonState(MouseButton.Left, true); // stays pressed (transitions to Pressed)
        world.Update();

        scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(scroll.Offset.Y > 0f,
            $"Expected Offset.Y > 0 after dragging vertical thumb down, got {scroll.Offset.Y}");
    }

    [Fact]
    public void HorizontalThumbDrag_UpdatesScrollOffset()
    {
        using var world = CreateWorld();
        var container = SpawnHorizontalScrollContainer(world, contentWidth: 400f);

        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.False(scroll.HorizontalThumbEntity.IsNull);

        var thumbComputed = world.Store.GetComponent<ComputedNode>(scroll.HorizontalThumbEntity);
        // Horizontal thumb is at bottom of container: Y ~192, X ~0, Size: 100x6

        var mouse = new Mouse(0);
        mouse.SetPosition((int)(thumbComputed.Position.X + 10), (int)(thumbComputed.Position.Y + 3));
        mouse.SetButtonState(MouseButton.Left, true);

        var mouseInput = world.Resources.Get<ButtonInput<MouseButton>>();
        mouseInput.AddDevice(mouse.Id, mouse);
        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;

        // Frame 1: capture
        world.Update();

        // Frame 2: drag right
        mouse.SetPosition((int)(thumbComputed.Position.X + 50), (int)(thumbComputed.Position.Y + 3));
        mouse.SetButtonState(MouseButton.Left, true);
        world.Update();

        scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.True(scroll.Offset.X > 0f,
            $"Expected Offset.X > 0 after dragging horizontal thumb right, got {scroll.Offset.X}");
    }

    [Fact]
    public void ColumnLayout_ChildWiderThanContainer_ContentSizeXExceedsContainerWidth()
    {
        // Simulates: Column container shrinks below child width (e.g. window resize)
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // Column container 200px wide, Overflow.X = Scroll
        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIScrollOffset(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Size = new Size<Length>(Length.Px(200), Length.Px(200)),
                    Overflow = new Point<Overflow>(Overflow.Scroll, Overflow.Scroll),
                }
            }
        )).Entity;

        // Child 300px wide — wider than container
        var child = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Px(300), Length.Px(40)),
                }
            }
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, child);
        world.Update();
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(container);
        Assert.True(computed.ContentSize.X > computed.Size.X,
            $"ContentSize.X ({computed.ContentSize.X}) should exceed Size.X ({computed.Size.X}) " +
            $"when child is wider than container");

        // Horizontal thumb should be spawned
        var scroll = world.Store.GetComponent<UIScrollOffset>(container);
        Assert.False(scroll.HorizontalThumbEntity.IsNull,
            "HorizontalThumbEntity should be spawned when content overflows horizontally");
    }

    [Fact]
    public void ColumnLayout_ScrollY_ContentSizeExceedsContainer()
    {
        // Test with FlexDirection.Column and many children (matching UIExample pattern)
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // Scroll panel: Column, Overflow.Y = Scroll, with padding and border
        var scrollPanel = commands.Spawn(Entity.With(
            new UINode(),
            new UIScrollOffset(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Size = new Size<Length>(Length.Px(400), Length.Px(300)),
                    Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Scroll),
                    Padding = Rect<Length>.All(16),
                    Border = Rect<Length>.All(2),
                    Gap = new Size<Length>(0, 12),
                }
            }
        )).Entity;

        // Add 10 children of 60px each = 600px + gaps + padding = well over 300px
        for (int i = 0; i < 10; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Auto, Length.Px(60)),
                    }
                }
            )).Entity;
            commands.AddChild(scrollPanel, child);
        }

        commands.AddChild(root, scrollPanel);
        world.Update();
        world.Update();

        var computed = world.Store.GetComponent<ComputedNode>(scrollPanel);
        var innerHeight = computed.Size.Y - computed.PaddingTop - computed.PaddingBottom
                          - computed.BorderTop - computed.BorderBottom;
        var maxScroll = computed.ContentSize.Y - innerHeight;

        Assert.True(computed.ContentSize.Y > computed.Size.Y,
            $"ContentSize.Y ({computed.ContentSize.Y}) should exceed Size.Y ({computed.Size.Y}) for overflow");
        Assert.True(maxScroll > 0,
            $"maxScroll ({maxScroll}) should be positive for scrollable content");

        // Now test scrolling works
        var mouse = new Mouse(0);
        mouse.SetPosition(200, 150); // within container bounds
        mouse.SetAxisState(MouseAxis.ScrollY, -1f); // scroll down

        world.Resources.Get<CurrentDevice<Mouse>>().Value = mouse;
        world.Update();

        var scroll = world.Store.GetComponent<UIScrollOffset>(scrollPanel);
        Assert.True(scroll.Offset.Y > 0f,
            $"Expected Offset.Y > 0 after scrolling down, got {scroll.Offset.Y}");
    }
}
