using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIRootBuilderTests
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
    public void Root_SpawnsWithUINodeAndUILayoutRootAndUIStyle()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Root(commands, 800, 600).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UILayoutRoot>(entity));
        Assert.True(world.Store.HasComponent<UIStyle>(entity));
    }

    [Fact]
    public void Root_SetsViewportSize()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Root(commands, 1920, 1080).Spawn();
        world.Update();

        var layoutRoot = world.Store.GetComponent<UILayoutRoot>(entity);
        Assert.Equal(1920f, layoutRoot.Size.Width);
        Assert.Equal(1080f, layoutRoot.Size.Height);
    }

    [Fact]
    public void Root_DefaultStyle_SetsFullSizeAndColumn()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Root(commands, 800, 600).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Percent(1f), style.Value.Size.Width);
        Assert.Equal(Length.Percent(1f), style.Value.Size.Height);
    }

    [Fact]
    public void Root_SupportsFluentChaining()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Root(commands, 800, 600)
            .FlexColumn()
            .Padding(16)
            .Gap(8)
            .Background(Color.BLACK)
            .Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(FlexDirection.Column, style.Value.FlexDirection);
        Assert.Equal(Length.Px(16), style.Value.Padding.Top);
        Assert.Equal(Length.Px(8), style.Value.Gap.Width);
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void Root_Children_AddsChildren()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var child1 = Panel(commands).Spawn();
        var child2 = Panel(commands).Spawn();

        var root = Root(commands, 800, 600).Children(child1, child2).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Parent>(root));
        var parentComp = world.Store.GetComponent<Parent>(root);
        Assert.Equal(2, parentComp.ChildCount);
    }
}
