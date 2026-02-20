namespace Pollus.Tests.UI.Builder;

using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;
using static Pollus.UI.UI;

public class UIBuilderIntegrationTests
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
    public void NestedChildren_CreatesCorrectHierarchy()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var child1 = Panel(commands).Size(100, 50).Spawn();
        var child2 = Text(commands, "Label").Spawn();

        var parent = Panel(commands)
            .FlexColumn()
            .Gap(8)
            .Children(child1, child2)
            .ChildOf(root)
            .Spawn();

        world.Update();

        // Parent has 2 children
        Assert.True(world.Store.HasComponent<Parent>(parent));
        var parentComp = world.Store.GetComponent<Parent>(parent);
        Assert.Equal(2, parentComp.ChildCount);

        // Parent is child of root
        Assert.True(world.Store.HasComponent<Child>(parent));
        Assert.Equal(root, world.Store.GetComponent<Child>(parent).Parent);

        // Children are children of parent
        Assert.True(world.Store.HasComponent<Child>(child1));
        Assert.Equal(parent, world.Store.GetComponent<Child>(child1).Parent);
        Assert.True(world.Store.HasComponent<Child>(child2));
        Assert.Equal(parent, world.Store.GetComponent<Child>(child2).Parent);
    }

    [Fact]
    public void MixedWidgets_UnderSameParent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var panel = Panel(commands).FlexColumn().Gap(8).ChildOf(root).Spawn();

        Text(commands, "Title").FontSize(24f).ChildOf(panel).Spawn();
        Button(commands).Size(100, 30).ChildOf(panel).Spawn();
        Toggle(commands).ChildOf(panel).Spawn();
        CheckBox(commands).ChildOf(panel).Spawn();

        world.Update();

        var parentComp = world.Store.GetComponent<Parent>(panel);
        Assert.Equal(4, parentComp.ChildCount);
    }

    [Fact]
    public void BuilderEntities_WorkWithLayoutPipeline()
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
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                    FlexDirection = FlexDirection.Column,
                }
            }
        )).Entity;

        var panel = Panel(commands)
            .Size(200, 100)
            .Background(Color.GRAY)
            .ChildOf(root)
            .Spawn();

        world.Update();

        // Run layout pipeline
        var adapter = world.Resources.Get<UITreeAdapter>();
        var uiNodeQuery = new Query<UINode>(world);
        var query = new Query(world);
        adapter.SyncFull(uiNodeQuery, query);

        if (adapter.IsDirty)
        {
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

                adapter.ComputeChildLayout(rootNodeId, input);
            }

            // WriteBack
            foreach (var entity in adapter.ActiveEntities)
            {
                int nodeId = adapter.GetNodeId(entity);
                if (nodeId < 0) continue;
                if (!query.Has<ComputedNode>(entity)) continue;
                ref readonly var rounded = ref adapter.GetRoundedLayout(nodeId);
                ref var computed = ref query.Get<ComputedNode>(entity);
                computed.Size = new Vec2f(rounded.Size.Width, rounded.Size.Height);
                computed.Position = new Vec2f(rounded.Location.X, rounded.Location.Y);
            }

            adapter.ClearDirty();
        }

        // Panel should have correct size after layout
        var panelComputed = world.Store.GetComponent<ComputedNode>(panel);
        Assert.Equal(200f, panelComputed.Size.X);
        Assert.Equal(100f, panelComputed.Size.Y);
    }

    [Fact]
    public void DropdownBuilder_WorksWithDropdownSystem()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var result = Dropdown(commands)
            .Size(200, 30)
            .Placeholder("Choose...")
            .Option("Alpha").Option("Beta").Option("Gamma")
            .ChildOf(root)
            .Spawn();

        world.Update();

        // Verify the dropdown system can interact with builder-created entities
        var qDropdown = new Query<UIDropdown>(world);
        var qDropdownOptions = new Query<UIDropdownOptionTag, UIStyle>(world);

        // Simulate click to open
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = result.Entity });

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        UIDropdownSystem.PerformUpdate(new(world), qDropdown, qDropdownOptions, clickReader, keyReader, world.Events);

        // Dropdown should be open
        var dd = world.Store.GetComponent<UIDropdown>(result.Entity);
        Assert.True(dd.IsOpen);

        // Options should now be visible
        foreach (var opt in result.OptionEntities)
        {
            var optStyle = world.Store.GetComponent<UIStyle>(opt);
            Assert.Equal(Display.Flex, optStyle.Value.Display);
        }
    }

    [Fact]
    public void SliderResult_CanBeUsedAsEntity()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        // SliderResult implicit conversion to Entity works for Children()
        SliderResult slider = Slider(commands).Size(200, 20).Spawn();
        Entity sliderEntity = slider;

        var panel = Panel(commands).Children(sliderEntity).ChildOf(root).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Parent>(panel));
        Assert.Equal(1, world.Store.GetComponent<Parent>(panel).ChildCount);
    }

    [Fact]
    public void TextInputResult_CanBeUsedAsEntity()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        TextInputResult input = TextInput(commands).Size(200, 30).Spawn();
        Entity inputEntity = input;

        var panel = Panel(commands).Children(inputEntity).ChildOf(root).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Parent>(panel));
        Assert.Equal(1, world.Store.GetComponent<Parent>(panel).ChildCount);
    }
}
