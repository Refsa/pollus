using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Engine.UI;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIDropdownTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Resources.Add(new UIHitTestResult());
        world.Resources.Add(new UIFocusState());
        world.Events.InitEvent<UIInteractionEvents.UIClickEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIHoverEnterEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIHoverExitEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIPressEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIReleaseEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIFocusEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIBlurEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIKeyDownEvent>();
        world.Events.InitEvent<UIDropdownEvents.UIDropdownSelectionChanged>();
        world.Prepare();
        return world;
    }

    static (Entity dropdown, Entity opt0, Entity opt1, Entity opt2) SpawnDropdown(World world)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var dropdown = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdown { SelectedIndex = -1 },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)) } }
        )).Entity;

        var opt0 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = 0 },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)) } }
        )).Entity;

        var opt1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = 1 },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)) } }
        )).Entity;

        var opt2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = 2 },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)) } }
        )).Entity;

        commands.AddChild(root, dropdown);
        commands.AddChild(root, opt0);
        commands.AddChild(root, opt1);
        commands.AddChild(root, opt2);
        world.Update();
        return (dropdown, opt0, opt1, opt2);
    }

    [Fact]
    public void Click_OpensDropdownAndShowsOptions()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, opt1, opt2) = SpawnDropdown(world);

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = dropdown });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        var state = world.Store.GetComponent<UIDropdown>(dropdown);
        Assert.True(state.IsOpen);

        // Options should now be visible
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt2).Value.Display);
    }

    [Fact]
    public void ClickOption_SelectsAndCloses()
    {
        using var world = CreateWorld();
        var (dropdown, _, opt1, _) = SpawnDropdown(world);

        // Open the dropdown first
        ref var dd = ref world.Store.GetComponent<UIDropdown>(dropdown);
        dd.IsOpen = true;

        // Click option 1
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = opt1 });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        dd = world.Store.GetComponent<UIDropdown>(dropdown);
        Assert.Equal(1, dd.SelectedIndex);
        Assert.False(dd.IsOpen);
    }

    [Fact]
    public void UIDropdownSelectionChanged_Emitted()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, _, _) = SpawnDropdown(world);

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = opt0 });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var ddReader = world.Events.GetReader<UIDropdownEvents.UIDropdownSelectionChanged>()!;

        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        var events = ddReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(dropdown, events[0].Entity);
        Assert.Equal(0, events[0].SelectedIndex);
        Assert.Equal(-1, events[0].PreviousIndex);
    }

    [Fact]
    public void Escape_ClosesDropdown()
    {
        using var world = CreateWorld();
        var (dropdown, _, _, _) = SpawnDropdown(world);

        ref var dd = ref world.Store.GetComponent<UIDropdown>(dropdown);
        dd.IsOpen = true;

        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = dropdown, Key = (int)Key.Escape });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        dd = world.Store.GetComponent<UIDropdown>(dropdown);
        Assert.False(dd.IsOpen);
    }

    [Fact]
    public void Click_TogglesOpenCloseAndVisibility()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, opt1, opt2) = SpawnDropdown(world);

        var query = new Query(world);
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        // Sync initial state - options hidden
        UIDropdownSystem.SyncOptionVisibility(query);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt0).Value.Display);

        // Click to open
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = dropdown });
        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        var state = world.Store.GetComponent<UIDropdown>(dropdown);
        Assert.True(state.IsOpen);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt2).Value.Display);

        // Click to close
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = dropdown });
        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        state = world.Store.GetComponent<UIDropdown>(dropdown);
        Assert.False(state.IsOpen);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt2).Value.Display);
    }

    [Fact]
    public void Options_HiddenWhenClosed()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, opt1, opt2) = SpawnDropdown(world);

        var query = new Query(world);

        // Dropdown starts closed - sync should hide options
        UIDropdownSystem.SyncOptionVisibility(query);

        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt2).Value.Display);
    }

    [Fact]
    public void Options_VisibleWhenOpen()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, opt1, opt2) = SpawnDropdown(world);

        ref var dd = ref world.Store.GetComponent<UIDropdown>(dropdown);
        dd.IsOpen = true;

        var query = new Query(world);
        UIDropdownSystem.SyncOptionVisibility(query);

        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt2).Value.Display);
    }

    [Fact]
    public void ClickOption_HidesOptionsAfterSelection()
    {
        using var world = CreateWorld();
        var (dropdown, opt0, opt1, opt2) = SpawnDropdown(world);

        // Open the dropdown
        ref var dd = ref world.Store.GetComponent<UIDropdown>(dropdown);
        dd.IsOpen = true;

        // Click option 1
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = opt1 });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        // Options should be hidden after selection closes dropdown
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt1).Value.Display);
        Assert.Equal(Display.None, world.Store.GetComponent<UIStyle>(opt2).Value.Display);
    }

    /// Matches the real UIRectExample: options start with Display.None,
    /// runs full layout pipeline, verifies ComputedNode sizes become non-zero.
    [Fact]
    public void FullPipeline_OptionsGetNonZeroSizeAfterOpen()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                    FlexDirection = FlexDirection.Column,
                }
            }
        )).Entity;

        var dropdown = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdown { SelectedIndex = -1 },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)),
                }
            }
        )).Entity;

        var opt0 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = 0 },
            new BackgroundColor { Color = new Pollus.Utils.Color(0.2f, 0.2f, 0.25f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Display = Display.None,
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                }
            }
        )).Entity;

        var opt1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = 1 },
            new BackgroundColor { Color = new Pollus.Utils.Color(0.2f, 0.2f, 0.25f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Display = Display.None,
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                }
            }
        )).Entity;

        commands.AddChild(root, dropdown);
        commands.AddChild(root, opt0);
        commands.AddChild(root, opt1);
        world.Update();

        // Run layout pipeline (frame 0: initial layout with options hidden)
        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);
        adapter.SyncFull(uiNodeQuery, query);

        // Run initial compute + writeback
        RunLayout(adapter, query);

        // Options should have zero size (Display.None)
        var opt0Computed = world.Store.GetComponent<ComputedNode>(opt0);
        Assert.Equal(0f, opt0Computed.Size.X);
        Assert.Equal(0f, opt0Computed.Size.Y);

        // --- Click to open dropdown ---
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = dropdown });

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        // Verify IsOpen toggled
        Assert.True(world.Store.GetComponent<UIDropdown>(dropdown).IsOpen);
        // Verify UIStyle.Display changed to Flex
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt1).Value.Display);

        // --- Next frame: layout picks up the change ---
        adapter.SyncFull(uiNodeQuery, query);
        Assert.True(adapter.IsDirty, "Adapter should be dirty after style change");

        RunLayout(adapter, query);

        // Options should now have non-zero size
        opt0Computed = world.Store.GetComponent<ComputedNode>(opt0);
        var opt1Computed = world.Store.GetComponent<ComputedNode>(opt1);
        Assert.True(opt0Computed.Size.X > 0, $"opt0 width should be > 0, got {opt0Computed.Size.X}");
        Assert.True(opt0Computed.Size.Y > 0, $"opt0 height should be > 0, got {opt0Computed.Size.Y}");
        Assert.True(opt1Computed.Size.X > 0, $"opt1 width should be > 0, got {opt1Computed.Size.X}");
        Assert.True(opt1Computed.Size.Y > 0, $"opt1 height should be > 0, got {opt1Computed.Size.Y}");
    }

    static void RunLayout(UITreeAdapter adapter, Query query)
    {
        if (!adapter.IsDirty) return;

        foreach (var rootNodeId in adapter.Roots)
        {
            var rootEntity = adapter.GetEntity(rootNodeId);
            if (!query.Has<UILayoutRoot>(rootEntity)) continue;

            ref readonly var layoutRoot = ref query.Get<UILayoutRoot>(rootEntity);
            float width = layoutRoot.Size.Width;
            float height = layoutRoot.Size.Height;

            ref readonly var rootStyle = ref adapter.GetStyle(rootNodeId);
            var parentSz = new Size<float?>(width, height);
            var adj = LayoutHelpers.ContentBoxAdjustment(
                rootStyle.BoxSizing,
                LayoutHelpers.ResolvePadding(rootStyle, parentSz),
                LayoutHelpers.ResolveBorder(rootStyle, parentSz));
            float innerW = width + (adj.Width ?? 0f);
            float innerH = height + (adj.Height ?? 0f);

            var input = new LayoutInput
            {
                RunMode = RunMode.PerformLayout,
                SizingMode = SizingMode.InherentSize,
                Axis = RequestedAxis.Both,
                KnownDimensions = new Size<float?>(innerW, innerH),
                ParentSize = new Size<float?>(width, height),
                AvailableSpace = new Size<AvailableSpace>(
                    AvailableSpace.Definite(width),
                    AvailableSpace.Definite(height)
                ),
            };

            var output = adapter.ComputeChildLayout(rootNodeId, input);
            var rootPadding = LayoutHelpers.ResolvePadding(rootStyle, parentSz);
            var rootBorder = LayoutHelpers.ResolveBorder(rootStyle, parentSz);
            ref var rootLayout = ref adapter.GetLayout(rootNodeId);
            rootLayout.Size = new Size<float>(
                innerW + rootPadding.HorizontalAxisSum() + rootBorder.HorizontalAxisSum(),
                innerH + rootPadding.VerticalAxisSum() + rootBorder.VerticalAxisSum());
            rootLayout.ContentSize = output.ContentSize;
            rootLayout.Padding = rootPadding;
            rootLayout.Border = rootBorder;
            rootLayout.Margin = LayoutHelpers.ResolveMargin(rootStyle, parentSz);
            adapter.GetUnroundedLayout(rootNodeId) = rootLayout;
            var tree = adapter;
            RoundLayout.Round(ref tree, rootNodeId);
        }

        // WriteBack
        var enumerator = adapter.GetActiveNodes();
        while (enumerator.MoveNext())
        {
            var (entity, nodeId) = enumerator.Current;
            if (!query.Has<ComputedNode>(entity)) continue;
            ref readonly var rounded = ref adapter.GetRoundedLayout(nodeId);
            ref var computed = ref query.Get<ComputedNode>(entity);
            computed.Size = new Vec2f(rounded.Size.Width, rounded.Size.Height);
            computed.Position = new Vec2f(rounded.Location.X, rounded.Location.Y);
        }

        adapter.ClearDirty();
    }

    [Fact]
    public void DropdownOptions_ResourceWorks()
    {
        var options = new UIDropdownOptions();
        options.Add("Option A");
        options.Add("Option B");
        options.Add("Option C");

        Assert.Equal(3, options.Count);
        Assert.Equal("Option A", options.Get(0));
        Assert.Equal("Option B", options.Get(1));
        Assert.Equal("Option C", options.Get(2));
        Assert.Equal("", options.Get(-1));
        Assert.Equal("", options.Get(5));
    }
}
