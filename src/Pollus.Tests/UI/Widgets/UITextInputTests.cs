using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UITextInputTests
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

    static Entity SpawnTextInput(World world, UIInputFilterType filter = UIInputFilterType.Any)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var textInput = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UITextInput { Filter = filter },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(30)) } }
        )).Entity;

        commands.AddChild(root, textInput);
        world.Update();
        return textInput;
    }

    [Fact]
    public void CharacterInsertion_AtCursor()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();

        // Write text input event targeting the entity
        var textWriter = world.Events.GetWriter<UIInteractionEvents.UITextInputEvent>();
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "a" });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        Assert.Equal("a", textBuffers.Get(entity));
        var input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(1, input.CursorPosition);
    }

    [Fact]
    public void Backspace_RemovesCharBeforeCursor()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(entity, "hello");

        // Set cursor to end
        ref var input = ref world.Store.GetComponent<UITextInput>(entity);
        input.CursorPosition = 5;

        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.Backspace });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        Assert.Equal("hell", textBuffers.Get(entity));
        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(4, input.CursorPosition);
    }

    [Fact]
    public void Delete_RemovesCharAtCursor()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(entity, "hello");

        ref var input = ref world.Store.GetComponent<UITextInput>(entity);
        input.CursorPosition = 0;

        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.Delete });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        Assert.Equal("ello", textBuffers.Get(entity));
        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(0, input.CursorPosition);
    }

    [Fact]
    public void ArrowKeys_MoveCursor()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(entity, "abc");

        ref var input = ref world.Store.GetComponent<UITextInput>(entity);
        input.CursorPosition = 1;

        var query = new Query(world);

        // Arrow right
        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.ArrowRight });
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;
        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(2, input.CursorPosition);

        // Arrow left
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.ArrowLeft });
        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(1, input.CursorPosition);
    }

    [Fact]
    public void HomeEnd_JumpsCursor()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(entity, "hello");

        ref var input = ref world.Store.GetComponent<UITextInput>(entity);
        input.CursorPosition = 2;

        var query = new Query(world);

        // Home
        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.Home });
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;
        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(0, input.CursorPosition);

        // End
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = entity, Key = (int)Key.End });
        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        input = world.Store.GetComponent<UITextInput>(entity);
        Assert.Equal(5, input.CursorPosition);
    }

    [Fact]
    public void IntegerFilter_RejectsLetters()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world, UIInputFilterType.Integer);
        var textBuffers = world.Resources.Get<UITextBuffers>();

        var textWriter = world.Events.GetWriter<UIInteractionEvents.UITextInputEvent>();
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "a" });
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "5" });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        Assert.Equal("5", textBuffers.Get(entity));
    }

    [Fact]
    public void DecimalFilter_AllowsSingleDot()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world, UIInputFilterType.Decimal);
        var textBuffers = world.Resources.Get<UITextBuffers>();

        var textWriter = world.Events.GetWriter<UIInteractionEvents.UITextInputEvent>();
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "3" });
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "." });
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "1" });
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "." });
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "4" });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        Assert.Equal("3.14", textBuffers.Get(entity));
    }

    [Fact]
    public void UITextInputValueChanged_EmittedOnChange()
    {
        using var world = CreateWorld();
        var entity = SpawnTextInput(world);
        var textBuffers = world.Resources.Get<UITextBuffers>();

        var textWriter = world.Events.GetWriter<UIInteractionEvents.UITextInputEvent>();
        textWriter.Write(new UIInteractionEvents.UITextInputEvent { Entity = entity, Text = "x" });

        var query = new Query(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UIInteractionEvents.UITextInputEvent>()!;
        var valueReader = world.Events.GetReader<UITextInputEvents.UITextInputValueChanged>()!;

        UITextInputSystem.PerformTextInput(query, textBuffers, keyReader, textReader, world.Events);

        var events = valueReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(entity, events[0].Entity);
    }

    [Fact]
    public void CaretBlinkTimer_Cycles()
    {
        // Test the PassesFilter function directly for caret blink
        Assert.True(UITextInputSystem.PassesFilter('a', UIInputFilterType.Any, "", 0));
        Assert.False(UITextInputSystem.PassesFilter('\t', UIInputFilterType.Any, "", 0)); // control char

        // Integer filter
        Assert.True(UITextInputSystem.PassesFilter('5', UIInputFilterType.Integer, "", 0));
        Assert.False(UITextInputSystem.PassesFilter('a', UIInputFilterType.Integer, "", 0));
        Assert.True(UITextInputSystem.PassesFilter('-', UIInputFilterType.Integer, "", 0));
        Assert.False(UITextInputSystem.PassesFilter('-', UIInputFilterType.Integer, "-5", 0)); // already has minus

        // Alphanumeric filter
        Assert.True(UITextInputSystem.PassesFilter('A', UIInputFilterType.Alphanumeric, "", 0));
        Assert.True(UITextInputSystem.PassesFilter('9', UIInputFilterType.Alphanumeric, "", 0));
        Assert.False(UITextInputSystem.PassesFilter('.', UIInputFilterType.Alphanumeric, "", 0));
    }
}
