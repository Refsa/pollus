using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIDropdownTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UISystemsPlugin(), addDependencies: true);
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

    /// Simulates exactly the real dropdown: absolute panel + options start Display.None,
    /// toggled to Flex, verifies options get non-overlapping stacked positions.
    [Fact]
    public void FullPipeline_AbsolutePanelOptionsStackAfterToggle()
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

        // Section container (like dropdownSection in UIRectExample)
        var section = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                }
            }
        )).Entity;

        var trigger = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdown { SelectedIndex = 0 },
            new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(32)),
                }
            }
        )).Entity;

        // Options panel - starts Display.None (exactly like UIRectExample)
        var panel = commands.Spawn(Entity.With(
            new UINode(),
            new UIDropdownOptionTag { DropdownEntity = trigger, OptionIndex = -1 },
            new BackgroundColor { Color = new Color(0.18f, 0.18f, 0.22f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Display = Display.None,
                    Position = Position.Absolute,
                    Inset = new Rect<LengthPercentageAuto>(
                        LengthPercentageAuto.Px(0),
                        LengthPercentageAuto.Auto,
                        LengthPercentageAuto.Px(40),
                        LengthPercentageAuto.Auto),
                    FlexDirection = FlexDirection.Column,
                    Padding = new Rect<LengthPercentage>(
                        LengthPercentage.Px(2), LengthPercentage.Px(2),
                        LengthPercentage.Px(2), LengthPercentage.Px(2)),
                    Gap = new Size<LengthPercentage>(LengthPercentage.Px(2), LengthPercentage.Px(2)),
                    Border = new Rect<LengthPercentage>(
                        LengthPercentage.Px(1), LengthPercentage.Px(1),
                        LengthPercentage.Px(1), LengthPercentage.Px(1)),
                }
            }
        )).Entity;

        // Options - start Display.None (exactly like UIRectExample)
        var opts = new Entity[4];
        for (int i = 0; i < 4; i++)
        {
            opts[i] = commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction { Focusable = true },
                new UIDropdownOptionTag { DropdownEntity = trigger, OptionIndex = i },
                new BackgroundColor { Color = new Color(0.2f, 0.2f, 0.25f, 1f) },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Display = Display.None,
                        Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                        Padding = new Rect<LengthPercentage>(
                            LengthPercentage.Px(10), LengthPercentage.Px(10),
                            LengthPercentage.Px(4), LengthPercentage.Px(4)),
                        AlignItems = AlignItems.Center,
                    }
                }
            )).Entity;
            commands.AddChild(panel, opts[i]);
        }

        // Later section (to verify z-order context)
        var laterSection = commands.Spawn(Entity.With(
            new UINode(),
            new BackgroundColor { Color = new Color(0.3f, 0.3f, 0.3f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
                }
            }
        )).Entity;

        commands.AddChild(section, trigger);
        commands.AddChild(section, panel);
        commands.AddChild(root, section);
        commands.AddChild(root, laterSection);
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);

        // === Frame 1: Initial layout (everything hidden) ===
        adapter.SyncFull(uiNodeQuery, query);
        RunLayout(adapter, query);

        // All options should be zero-sized
        for (int i = 0; i < 4; i++)
        {
            var comp = world.Store.GetComponent<ComputedNode>(opts[i]);
            Assert.Equal(0f, comp.Size.X);
            Assert.Equal(0f, comp.Size.Y);
        }

        // === Toggle: simulate UIDropdownSystem opening the dropdown ===
        // Panel: Display.None → Display.Flex
        ref var panelStyle = ref world.Store.GetComponent<UIStyle>(panel);
        panelStyle.Value = panelStyle.Value with { Display = Display.Flex };
        // Options: Display.None → Display.Flex
        for (int i = 0; i < 4; i++)
        {
            ref var optStyle = ref world.Store.GetComponent<UIStyle>(opts[i]);
            optStyle.Value = optStyle.Value with { Display = Display.Flex };
        }

        // === Frame 2: Layout picks up Display changes ===
        adapter.SyncFull(uiNodeQuery, query);
        Assert.True(adapter.IsDirty, "Adapter should be dirty after Display change");
        RunLayout(adapter, query);

        // Panel should have non-zero size
        var panelComp = world.Store.GetComponent<ComputedNode>(panel);
        Assert.True(panelComp.Size.X > 0, $"Panel width = {panelComp.Size.X}");
        Assert.True(panelComp.Size.Y > 0, $"Panel height = {panelComp.Size.Y}");
        Assert.Equal(40f, panelComp.Position.Y); // inset top

        // Options should have correct non-overlapping stacked positions
        var positions = new float[4];
        var heights = new float[4];
        for (int i = 0; i < 4; i++)
        {
            var comp = world.Store.GetComponent<ComputedNode>(opts[i]);
            Assert.True(comp.Size.X > 0, $"opt{i} width = {comp.Size.X}");
            Assert.True(comp.Size.Y > 0, $"opt{i} height = {comp.Size.Y}");
            positions[i] = comp.Position.Y;
            heights[i] = comp.Size.Y;
        }

        // Each option should be at a unique, increasing Y position
        for (int i = 1; i < 4; i++)
        {
            Assert.True(positions[i] > positions[i - 1],
                $"opt{i} Y ({positions[i]}) should be > opt{i - 1} Y ({positions[i - 1]})");
        }

        // No overlap: each Y >= prev Y + prev height
        for (int i = 1; i < 4; i++)
        {
            Assert.True(positions[i] >= positions[i - 1] + heights[i - 1],
                $"opt{i} Y ({positions[i]}) should be >= opt{i - 1} bottom ({positions[i - 1] + heights[i - 1]})");
        }
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

    /// Simulates the exact system execution order and verifies that the rendering
    /// tree walk would emit draw calls for option entities after the dropdown opens.
    [Fact]
    public void FullPipeline_RenderingTreeWalkEmitsDrawCalls()
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
            new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
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
            new BackgroundColor { Color = new Color(0.2f, 0.2f, 0.25f, 1f) },
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
            new BackgroundColor { Color = new Color(0.2f, 0.2f, 0.25f, 1f) },
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

        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);

        // === Frame 1: Initial layout (options hidden) ===
        adapter.SyncFull(uiNodeQuery, query);
        RunLayout(adapter, query);

        // Verify options are zero-sized
        Assert.Equal(0f, world.Store.GetComponent<ComputedNode>(opt0).Size.X);
        Assert.Equal(0f, world.Store.GetComponent<ComputedNode>(opt1).Size.X);

        // Simulate rendering tree walk for initial state
        var drawCalls = SimulateRenderingTreeWalk(query, root);
        Assert.DoesNotContain(opt0, drawCalls);
        Assert.DoesNotContain(opt1, drawCalls);
        Assert.Contains(dropdown, drawCalls); // trigger renders

        // === Frame 2: Click to open dropdown ===
        // Step 1-2: HitTest + UpdateState (simulated by directly writing click event)
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = dropdown });

        // Step 3: DropdownSystem processes click
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        UIDropdownSystem.PerformUpdate(query, clickReader, keyReader, world.Events);

        // Verify state changes
        Assert.True(world.Store.GetComponent<UIDropdown>(dropdown).IsOpen);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt0).Value.Display);
        Assert.Equal(Display.Flex, world.Store.GetComponent<UIStyle>(opt1).Value.Display);

        // Step 4: SyncTree detects style changes
        adapter.SyncFull(uiNodeQuery, query);
        Assert.True(adapter.IsDirty, "Tree should be dirty after Display change");

        // Step 5-6: ComputeLayout + WriteBack
        RunLayout(adapter, query);

        // Verify non-zero sizes
        var opt0Computed = world.Store.GetComponent<ComputedNode>(opt0);
        var opt1Computed = world.Store.GetComponent<ComputedNode>(opt1);
        Assert.True(opt0Computed.Size.X > 0, $"opt0 width = {opt0Computed.Size.X}");
        Assert.True(opt0Computed.Size.Y > 0, $"opt0 height = {opt0Computed.Size.Y}");
        Assert.True(opt1Computed.Size.X > 0, $"opt1 width = {opt1Computed.Size.X}");
        Assert.True(opt1Computed.Size.Y > 0, $"opt1 height = {opt1Computed.Size.Y}");

        // Step 7: Simulate rendering tree walk (ExtractUIRects.EmitNode equivalent)
        drawCalls = SimulateRenderingTreeWalk(query, root);
        Assert.Contains(dropdown, drawCalls);
        Assert.Contains(opt0, drawCalls);
        Assert.Contains(opt1, drawCalls);

        // Verify positions make sense (options below dropdown trigger)
        var ddComputed = world.Store.GetComponent<ComputedNode>(dropdown);
        Assert.True(opt0Computed.Position.Y >= ddComputed.Position.Y + ddComputed.Size.Y,
            $"opt0 Y ({opt0Computed.Position.Y}) should be below dropdown bottom ({ddComputed.Position.Y + ddComputed.Size.Y})");
    }

    /// Verifies that dropdown options in an absolute Column panel get non-overlapping
    /// stacked positions, and that the panel gets deferred rendering (higher sort index).
    [Fact]
    public void AbsolutePanel_OptionsStackWithoutOverlap()
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

        // Section container (like dropdownSection)
        var section = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                }
            }
        )).Entity;

        // Trigger button
        var trigger = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIDropdown { SelectedIndex = 0, IsOpen = true },
            new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(32)),
                }
            }
        )).Entity;

        // Absolute options panel
        var panel = commands.Spawn(Entity.With(
            new UINode(),
            new UIDropdownOptionTag { DropdownEntity = trigger, OptionIndex = -1 },
            new BackgroundColor { Color = new Color(0.18f, 0.18f, 0.22f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Display = Display.Flex,
                    Position = Position.Absolute,
                    Inset = new Rect<LengthPercentageAuto>(
                        LengthPercentageAuto.Px(0),
                        LengthPercentageAuto.Auto,
                        LengthPercentageAuto.Px(40),
                        LengthPercentageAuto.Auto),
                    FlexDirection = FlexDirection.Column,
                }
            }
        )).Entity;

        // Options
        var opts = new Entity[4];
        for (int i = 0; i < 4; i++)
        {
            opts[i] = commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction { Focusable = true },
                new UIDropdownOptionTag { DropdownEntity = trigger, OptionIndex = i },
                new BackgroundColor { Color = new Color(0.2f, 0.2f, 0.25f, 1f) },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Display = Display.Flex,
                        Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                    }
                }
            )).Entity;
            commands.AddChild(panel, opts[i]);
        }

        // A sibling section after dropdown (to test z-ordering)
        var laterSection = commands.Spawn(Entity.With(
            new UINode(),
            new BackgroundColor { Color = new Color(0.3f, 0.3f, 0.3f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
                }
            }
        )).Entity;

        commands.AddChild(section, trigger);
        commands.AddChild(section, panel);
        commands.AddChild(root, section);
        commands.AddChild(root, laterSection);
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);

        adapter.SyncFull(uiNodeQuery, query);
        RunLayout(adapter, query);

        // Verify options have correct non-overlapping positions
        var positions = new float[4];
        for (int i = 0; i < 4; i++)
        {
            var comp = world.Store.GetComponent<ComputedNode>(opts[i]);
            Assert.True(comp.Size.X > 0, $"opt{i} width should be > 0");
            Assert.True(comp.Size.Y > 0, $"opt{i} height should be > 0");
            positions[i] = comp.Position.Y;
        }

        // Each option should be at a unique Y position, increasing
        for (int i = 1; i < 4; i++)
        {
            Assert.True(positions[i] > positions[i - 1],
                $"opt{i} Y ({positions[i]}) should be > opt{i - 1} Y ({positions[i - 1]})");
        }

        // Options should not overlap: each Y should be >= prev Y + prev height
        for (int i = 1; i < 4; i++)
        {
            var prevComp = world.Store.GetComponent<ComputedNode>(opts[i - 1]);
            Assert.True(positions[i] >= positions[i - 1] + prevComp.Size.Y,
                $"opt{i} Y ({positions[i]}) should be >= opt{i - 1} bottom ({positions[i - 1] + prevComp.Size.Y})");
        }

        // Verify panel is at expected absolute position (Y=40 from inset)
        var panelComp = world.Store.GetComponent<ComputedNode>(panel);
        Assert.Equal(40f, panelComp.Position.Y);
        Assert.True(panelComp.Size.Y > 0, "Panel should have non-zero height");

        // Verify z-ordering: simulate tree walk with deferred absolute rendering
        var sortedEntities = SimulateDeferredRenderingWalk(query, root);
        int panelIndex = sortedEntities.IndexOf(panel);
        int laterIndex = sortedEntities.IndexOf(laterSection);
        Assert.True(panelIndex > laterIndex,
            $"Absolute panel (index {panelIndex}) should render after laterSection (index {laterIndex})");
    }

    /// Simulates the deferred rendering tree walk: normal flow first, absolute last.
    static List<Entity> SimulateDeferredRenderingWalk(Query query, Entity root)
    {
        var drawOrder = new List<Entity>();
        var deferred = new List<(Entity entity, Vec2f parentAbsPos)>();
        ref readonly var rootComputed = ref query.Get<ComputedNode>(root);
        SimulateDeferredEmitNode(query, drawOrder, root, rootComputed, Vec2f.Zero, deferred);

        // Process deferred absolute nodes
        foreach (var (entity, parentAbsPos) in deferred)
        {
            if (!query.Has<ComputedNode>(entity)) continue;
            ref readonly var computed = ref query.Get<ComputedNode>(entity);
            SimulateDeferredEmitNode(query, drawOrder, entity, computed, parentAbsPos, null);
        }

        return drawOrder;
    }

    static void SimulateDeferredEmitNode(Query query, List<Entity> drawOrder, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, List<(Entity entity, Vec2f parentAbsPos)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;

        if (size.X > 0 && size.Y > 0)
        {
            var entRef = query.GetEntity(entity);
            if (entRef.Has<BackgroundColor>())
                drawOrder.Add(entity);
        }

        if (size.X <= 0 && size.Y <= 0) return;

        var eRef = query.GetEntity(entity);
        if (!eRef.Has<Parent>()) return;

        var childEntity = eRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            if (childRef.Has<ComputedNode>())
            {
                if (deferred != null && childRef.Has<UIStyle>()
                    && childRef.Get<UIStyle>().Value.Position == Position.Absolute)
                {
                    deferred.Add((childEntity, absPos));
                }
                else
                {
                    ref var childComputed = ref childRef.Get<ComputedNode>();
                    SimulateDeferredEmitNode(query, drawOrder, childEntity, childComputed, absPos, deferred);
                }
            }
            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }

    /// Simulates the ExtractUIRects.EmitNode tree walk, collecting entities that
    /// would produce draw calls (non-zero size + BackgroundColor or BorderColor).
    static List<Entity> SimulateRenderingTreeWalk(Query query, Entity root)
    {
        var drawCalls = new List<Entity>();
        ref readonly var rootComputed = ref query.Get<ComputedNode>(root);
        SimulateEmitNode(query, drawCalls, root, rootComputed, Vec2f.Zero);
        return drawCalls;
    }

    static void SimulateEmitNode(Query query, List<Entity> drawCalls, Entity entity, in ComputedNode computed, Vec2f parentAbsPos)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;

        if (size.X > 0 && size.Y > 0)
        {
            var entRef = query.GetEntity(entity);
            if (entRef.Has<BackgroundColor>())
            {
                drawCalls.Add(entity);
            }
        }

        // Walk children (same as ExtractUIRects)
        var eRef = query.GetEntity(entity);
        if (!eRef.Has<Parent>()) return;

        var childEntity = eRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            if (childRef.Has<ComputedNode>())
            {
                ref var childComputed = ref childRef.Get<ComputedNode>();
                SimulateEmitNode(query, drawCalls, childEntity, childComputed, absPos);
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }

    /// Verifies that flex children with their own children (text-like nodes)
    /// get correct sizes inside an auto-sized absolute panel.
    /// Regression test for double-counting of padding+border and incorrect
    /// shrinking when the container has indefinite main size.
    [Fact]
    public void AbsolutePanel_OptionsWithChildrenGetExactSizes()
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

        // Section container (like dropdownSection)
        var section = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                }
            }
        )).Entity;

        // Trigger button (32px tall, gives section a small height)
        var trigger = commands.Spawn(Entity.With(
            new UINode(),
            new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(32)),
                }
            }
        )).Entity;

        // Absolute options panel - auto-sized, with padding+border
        var panel = commands.Spawn(Entity.With(
            new UINode(),
            new BackgroundColor { Color = new Color(0.18f, 0.18f, 0.22f, 1f) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Position = Position.Absolute,
                    Inset = new Rect<LengthPercentageAuto>(
                        LengthPercentageAuto.Px(0),
                        LengthPercentageAuto.Auto,
                        LengthPercentageAuto.Px(40),
                        LengthPercentageAuto.Auto),
                    FlexDirection = FlexDirection.Column,
                    Padding = new Rect<LengthPercentage>(
                        LengthPercentage.Px(2), LengthPercentage.Px(2),
                        LengthPercentage.Px(2), LengthPercentage.Px(2)),
                    Gap = new Size<LengthPercentage>(LengthPercentage.Px(2), LengthPercentage.Px(2)),
                    Border = new Rect<LengthPercentage>(
                        LengthPercentage.Px(1), LengthPercentage.Px(1),
                        LengthPercentage.Px(1), LengthPercentage.Px(1)),
                }
            }
        )).Entity;

        // Options with explicit 200x28 size, padding, and a child node each
        var opts = new Entity[4];
        var children = new Entity[4];
        for (int i = 0; i < 4; i++)
        {
            opts[i] = commands.Spawn(Entity.With(
                new UINode(),
                new BackgroundColor { Color = new Color(0.2f, 0.2f, 0.25f, 1f) },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                        Padding = new Rect<LengthPercentage>(
                            LengthPercentage.Px(10), LengthPercentage.Px(10),
                            LengthPercentage.Px(4), LengthPercentage.Px(4)),
                        AlignItems = AlignItems.Center,
                    }
                }
            )).Entity;

            // Child node (simulating text label) - uses percentage sizing
            children[i] = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                    }
                }
            )).Entity;

            commands.AddChild(opts[i], children[i]);
            commands.AddChild(panel, opts[i]);
        }

        commands.AddChild(section, trigger);
        commands.AddChild(section, panel);
        commands.AddChild(root, section);
        world.Update();

        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);

        adapter.SyncFull(uiNodeQuery, query);
        RunLayout(adapter, query);

        // Verify each option has the correct border-box size: 200x28
        for (int i = 0; i < 4; i++)
        {
            var comp = world.Store.GetComponent<ComputedNode>(opts[i]);
            Assert.True(MathF.Abs(comp.Size.X - 200f) < 1f,
                $"opt{i} width should be 200, got {comp.Size.X}");
            Assert.True(MathF.Abs(comp.Size.Y - 28f) < 1f,
                $"opt{i} height should be 28, got {comp.Size.Y}");
        }

        // Verify correct stacking: each option at increasing Y
        for (int i = 1; i < 4; i++)
        {
            var prev = world.Store.GetComponent<ComputedNode>(opts[i - 1]);
            var curr = world.Store.GetComponent<ComputedNode>(opts[i]);
            Assert.True(curr.Position.Y >= prev.Position.Y + prev.Size.Y,
                $"opt{i} Y ({curr.Position.Y}) should be >= opt{i - 1} bottom ({prev.Position.Y + prev.Size.Y})");
        }

        // Panel height should be: 4*28 + 3*2(gap) + 4(pad) + 2(border) = 124
        var panelComp = world.Store.GetComponent<ComputedNode>(panel);
        Assert.True(MathF.Abs(panelComp.Size.Y - 124f) < 1f,
            $"Panel height should be 124, got {panelComp.Size.Y}");
    }
}
