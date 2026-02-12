using Pollus.ECS;
using Pollus.Engine.Input;

namespace Pollus.Tests.Input;

public class TextInputEventTests
{
    [Fact]
    public void TextInputEvent_IsPublished_WhenKeyboardEnqueuesText()
    {
        using var world = new World();
        world.Events.InitEvent<ButtonEvent<Key>>();
        world.Events.InitEvent<TextInputEvent>();
        world.Prepare();

        var keyboard = new Keyboard();
        keyboard.EnqueueTextInput("a");
        keyboard.Update(world.Events);

        var reader = world.Events.GetReader<TextInputEvent>()!;
        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal("a", events[0].Text);
    }

    [Fact]
    public void TextInputEvent_MultipleCharacters_AllPublished()
    {
        using var world = new World();
        world.Events.InitEvent<ButtonEvent<Key>>();
        world.Events.InitEvent<TextInputEvent>();
        world.Prepare();

        var keyboard = new Keyboard();
        keyboard.EnqueueTextInput("h");
        keyboard.EnqueueTextInput("i");
        keyboard.EnqueueTextInput("!");
        keyboard.Update(world.Events);

        var reader = world.Events.GetReader<TextInputEvent>()!;
        var events = reader.Read();
        Assert.Equal(3, events.Length);
        Assert.Equal("h", events[0].Text);
        Assert.Equal("i", events[1].Text);
        Assert.Equal("!", events[2].Text);
    }

    [Fact]
    public void TextInputEvent_ClearedAfterUpdate()
    {
        using var world = new World();
        world.Events.InitEvent<ButtonEvent<Key>>();
        world.Events.InitEvent<TextInputEvent>();
        world.Prepare();

        var keyboard = new Keyboard();
        keyboard.EnqueueTextInput("x");
        keyboard.Update(world.Events);

        // Queue should be cleared after Update
        keyboard.Update(world.Events);

        var reader = world.Events.GetReader<TextInputEvent>()!;
        // First read consumes the "x" from first update
        reader.Read();

        // Second update should have no new events
        // Need a world update to clear the event buffer
        world.Update();
        var events2 = reader.Read();
        Assert.Equal(0, events2.Length);
    }

    [Fact]
    public void TextInputEvent_KeyRepeat_DoesNotAffectTextInput()
    {
        using var world = new World();
        world.Events.InitEvent<ButtonEvent<Key>>();
        world.Events.InitEvent<TextInputEvent>();
        world.Prepare();

        var keyboard = new Keyboard();

        // Simulate key press (from keydown) AND text input (from SDL_TEXTINPUT)
        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.EnqueueTextInput("a");
        keyboard.Update(world.Events);

        // Should have both a ButtonEvent<Key> and TextInputEvent
        var keyReader = world.Events.GetReader<ButtonEvent<Key>>()!;
        var textReader = world.Events.GetReader<TextInputEvent>()!;

        var keyEvents = keyReader.Read();
        var textEvents = textReader.Read();

        Assert.Equal(1, keyEvents.Length);
        Assert.Equal(1, textEvents.Length);
        Assert.Equal("a", textEvents[0].Text);
    }
}
