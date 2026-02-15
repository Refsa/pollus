using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UITextInputBuilderTests
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
    public void TextInput_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Size(200, 30).Spawn();
        world.Update();

        Entity entity = result;
        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UITextInput>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void TextInput_CreatesTextChild()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Size(200, 30).Spawn();
        world.Update();

        Assert.False(result.TextEntity.IsNull);
        Assert.True(world.Store.HasComponent<UIText>(result.TextEntity));
        Assert.True(world.Store.HasComponent<ContentSize>(result.TextEntity));
    }

    [Fact]
    public void TextInput_TextEntityIsChild()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Size(200, 30).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(result.TextEntity));
        Assert.Equal(result.Entity, world.Store.GetComponent<Child>(result.TextEntity).Parent);
    }

    [Fact]
    public void TextInput_SetsInitialText()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Text("Hello").Size(200, 30).Spawn();
        world.Update();

        var uiText = world.Store.GetComponent<UIText>(result.TextEntity);
        Assert.Equal("Hello", uiText.Text.ToString().TrimEnd('\0'));
    }

    [Fact]
    public void TextInput_SetsTextBuffer()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Text("Buffer test").Size(200, 30).Spawn();
        world.Update();

        var bufs = world.Resources.Get<UITextBuffers>();
        Assert.Equal("Buffer test", bufs.Get(result.Entity));
    }

    [Fact]
    public void TextInput_TextEntityReferencedInComponent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Size(200, 30).Spawn();
        world.Update();

        var textInput = world.Store.GetComponent<UITextInput>(result.Entity);
        Assert.Equal(result.TextEntity, textInput.TextEntity);
    }

    [Fact]
    public void TextInput_Filter_SetsFilterType()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Filter(UIInputFilterType.Integer).Size(200, 30).Spawn();
        world.Update();

        var textInput = world.Store.GetComponent<UITextInput>(result.Entity);
        Assert.Equal(UIInputFilterType.Integer, textInput.Filter);
    }

    [Fact]
    public void TextInput_IsFocusable()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = TextInput(commands).Size(200, 30).Spawn();
        world.Update();

        var interaction = world.Store.GetComponent<UIInteraction>(result.Entity);
        Assert.True(interaction.Focusable);
    }
}
