using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UIRadioButtonTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    static (Entity r1, Entity r2, Entity r3) SpawnRadioGroup(World world, int groupId)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var r1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIRadioButton { GroupId = groupId, IsSelected = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(24), Dimension.Px(24)) } }
        )).Entity;

        var r2 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIRadioButton { GroupId = groupId, IsSelected = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(24), Dimension.Px(24)) } }
        )).Entity;

        var r3 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIRadioButton { GroupId = groupId, IsSelected = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(24), Dimension.Px(24)) } }
        )).Entity;

        commands.AddChild(root, r1);
        commands.AddChild(root, r2);
        commands.AddChild(root, r3);
        world.Update();
        return (r1, r2, r3);
    }

    [Fact]
    public void Click_SelectsRadioButton()
    {
        using var world = CreateWorld();
        var (r1, _, _) = SpawnRadioGroup(world, 1);

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r1 });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;

        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        var state = world.Store.GetComponent<UIRadioButton>(r1);
        Assert.True(state.IsSelected);
    }

    [Fact]
    public void SelectingOne_DeselectsOthersInSameGroup()
    {
        using var world = CreateWorld();
        var (r1, r2, r3) = SpawnRadioGroup(world, 1);

        var query = new Query(world);

        // Select r1 first
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r1 });
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        Assert.True(world.Store.GetComponent<UIRadioButton>(r1).IsSelected);

        // Now select r2
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r2 });
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        Assert.False(world.Store.GetComponent<UIRadioButton>(r1).IsSelected);
        Assert.True(world.Store.GetComponent<UIRadioButton>(r2).IsSelected);
        Assert.False(world.Store.GetComponent<UIRadioButton>(r3).IsSelected);
    }

    [Fact]
    public void DifferentGroups_AreIndependent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var g1r1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIRadioButton { GroupId = 1, IsSelected = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(24), Dimension.Px(24)) } }
        )).Entity;

        var g2r1 = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UIRadioButton { GroupId = 2, IsSelected = false },
            new BackgroundColor(),
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(24), Dimension.Px(24)) } }
        )).Entity;

        commands.AddChild(root, g1r1);
        commands.AddChild(root, g2r1);
        world.Update();

        var query = new Query(world);

        // Select in group 1
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = g1r1 });
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        // Select in group 2
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = g2r1 });
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        // Both should be selected (different groups)
        Assert.True(world.Store.GetComponent<UIRadioButton>(g1r1).IsSelected);
        Assert.True(world.Store.GetComponent<UIRadioButton>(g2r1).IsSelected);
    }

    [Fact]
    public void UIRadioButtonEvent_Emitted()
    {
        using var world = CreateWorld();
        var (r1, _, _) = SpawnRadioGroup(world, 1);

        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r1 });

        var query = new Query(world);
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var radioReader = world.Events.GetReader<UIRadioButtonEvents.UIRadioButtonEvent>()!;

        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        var events = radioReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(r1, events[0].Entity);
        Assert.Equal(1, events[0].GroupId);
        Assert.True(events[0].IsSelected);
    }

    [Fact]
    public void AlreadySelected_StaysSelectedOnReclick()
    {
        using var world = CreateWorld();
        var (r1, _, _) = SpawnRadioGroup(world, 1);

        var query = new Query(world);
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var radioReader = world.Events.GetReader<UIRadioButtonEvents.UIRadioButtonEvent>()!;

        // Select r1
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r1 });
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);
        radioReader.Read(); // consume

        // Click r1 again
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = r1 });
        UIRadioButtonSystem.UpdateRadioButtons(query, clickReader, world.Events);

        Assert.True(world.Store.GetComponent<UIRadioButton>(r1).IsSelected);

        // No new event should be emitted for re-click
        var events = radioReader.Read();
        Assert.Equal(0, events.Length);
    }
}
