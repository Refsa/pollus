using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIToggleBuilderTests
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
    public void Toggle_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Toggle(commands).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UIToggle>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void Toggle_IsOn_SetsInitialState()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Toggle(commands).IsOn(true).Spawn();
        world.Update();

        var toggle = world.Store.GetComponent<UIToggle>(entity);
        Assert.True(toggle.IsOn);
    }

    [Fact]
    public void Toggle_DefaultIsOff()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Toggle(commands).Spawn();
        world.Update();

        var toggle = world.Store.GetComponent<UIToggle>(entity);
        Assert.False(toggle.IsOn);
    }

    [Fact]
    public void Toggle_CustomColors()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Toggle(commands)
            .OnColor(Color.GREEN)
            .OffColor(Color.RED)
            .Spawn();
        world.Update();

        var toggle = world.Store.GetComponent<UIToggle>(entity);
        Assert.Equal(Color.GREEN, toggle.OnColor);
        Assert.Equal(Color.RED, toggle.OffColor);
    }
}
