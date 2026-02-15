using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIButtonBuilderTests
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
    public void Button_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Button(commands).Size(100, 30).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UIButton>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
        Assert.True(world.Store.HasComponent<UIStyle>(entity));
    }

    [Fact]
    public void Button_Colors_SetsNormalColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Button(commands)
            .Colors(Color.BLUE, hover: Color.CYAN)
            .Spawn();
        world.Update();

        var btn = world.Store.GetComponent<UIButton>(entity);
        Assert.Equal(Color.BLUE, btn.NormalColor);
        Assert.Equal(Color.CYAN, btn.HoverColor);
    }

    [Fact]
    public void Button_Colors_SetsAllColors()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Button(commands)
            .Colors(Color.BLUE, Color.CYAN, Color.RED, Color.GRAY)
            .Spawn();
        world.Update();

        var btn = world.Store.GetComponent<UIButton>(entity);
        Assert.Equal(Color.BLUE, btn.NormalColor);
        Assert.Equal(Color.CYAN, btn.HoverColor);
        Assert.Equal(Color.RED, btn.PressedColor);
        Assert.Equal(Color.GRAY, btn.DisabledColor);
    }

    [Fact]
    public void Button_Size_SetsStyle()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Button(commands).Size(100, 30).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(100), style.Value.Size.Width);
        Assert.Equal(Length.Px(30), style.Value.Size.Height);
    }

    [Fact]
    public void Button_ChildOf_SetsParent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var btn = Button(commands).Size(100, 30).ChildOf(root).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(btn));
        Assert.Equal(root, world.Store.GetComponent<Child>(btn).Parent);
    }
}
