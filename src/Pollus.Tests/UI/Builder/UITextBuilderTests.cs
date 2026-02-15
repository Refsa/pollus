using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UITextBuilderTests
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
    public void Text_SpawnsWithUITextAndContentSize()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Hello").Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UIText>(entity));
        Assert.True(world.Store.HasComponent<ContentSize>(entity));
    }

    [Fact]
    public void Text_SetsTextContent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Hello World").Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(entity);
        Assert.Equal("Hello World", uiText.Text.ToString().TrimEnd('\0'));
    }

    [Fact]
    public void Text_FontSize_SetsSize()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Test").FontSize(24f).Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(entity);
        Assert.Equal(24f, uiText.Size);
    }

    [Fact]
    public void Text_Color_SetsColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Test").Color(Color.RED).Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(entity);
        Assert.Equal(Color.RED, uiText.Color);
    }

    [Fact]
    public void Text_DefaultFontSizeIs16()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Test").Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(entity);
        Assert.Equal(16f, uiText.Size);
    }

    [Fact]
    public void Text_DefaultColorIsWhite()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Text(commands, "Test").Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(entity);
        Assert.Equal(Color.WHITE, uiText.Color);
    }

    [Fact]
    public void Text_ChildOf_SetsParent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var text = Text(commands, "Hello").ChildOf(root).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(text));
        Assert.Equal(root, world.Store.GetComponent<Child>(text).Parent);
    }
}
