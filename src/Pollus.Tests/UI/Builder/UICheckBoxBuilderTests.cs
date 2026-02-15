using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UICheckBoxBuilderTests
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
    public void CheckBox_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = CheckBox(commands).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UICheckBox>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void CheckBox_IsChecked_SetsInitialState()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = CheckBox(commands).IsChecked(true).Spawn();
        world.Update();

        var cb = world.Store.GetComponent<UICheckBox>(entity);
        Assert.True(cb.IsChecked);
    }

    [Fact]
    public void CheckBox_DefaultIsUnchecked()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = CheckBox(commands).Spawn();
        world.Update();

        var cb = world.Store.GetComponent<UICheckBox>(entity);
        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void CheckBox_CustomColors()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = CheckBox(commands)
            .CheckedColor(Color.GREEN)
            .UncheckedColor(Color.RED)
            .CheckmarkColor(Color.BLACK)
            .Spawn();
        world.Update();

        var cb = world.Store.GetComponent<UICheckBox>(entity);
        Assert.Equal(Color.GREEN, cb.CheckedColor);
        Assert.Equal(Color.RED, cb.UncheckedColor);
        Assert.Equal(Color.BLACK, cb.CheckmarkColor);
    }
}
