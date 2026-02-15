using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIRadioButtonBuilderTests
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
    public void RadioButton_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = RadioButton(commands, 1).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UIRadioButton>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void RadioButton_SetsGroupId()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = RadioButton(commands, 42).Spawn();
        world.Update();

        var rb = world.Store.GetComponent<UIRadioButton>(entity);
        Assert.Equal(42, rb.GroupId);
    }

    [Fact]
    public void RadioButton_IsSelected_SetsInitialState()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = RadioButton(commands, 1).IsSelected(true).Spawn();
        world.Update();

        var rb = world.Store.GetComponent<UIRadioButton>(entity);
        Assert.True(rb.IsSelected);
    }

    [Fact]
    public void RadioButton_CustomColors()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = RadioButton(commands, 1)
            .SelectedColor(Color.BLUE)
            .UnselectedColor(Color.GRAY)
            .Spawn();
        world.Update();

        var rb = world.Store.GetComponent<UIRadioButton>(entity);
        Assert.Equal(Color.BLUE, rb.SelectedColor);
        Assert.Equal(Color.GRAY, rb.UnselectedColor);
    }
}
