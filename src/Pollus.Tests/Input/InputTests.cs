namespace Pollus.Tests.Input;

using Pollus.ECS;
using Pollus.Engine.Input;

public class InputTests
{
    [Fact]
    public void Test_Keyboard()
    {
        var events = new Events();
        events.InitEvent<ButtonEvent<MouseButton>>();
        events.InitEvent<AxisEvent<MouseAxis>>();
        events.InitEvent<ButtonEvent<GamepadButton>>();
        events.InitEvent<AxisEvent<GamepadAxis>>();
        events.InitEvent<ButtonEvent<Key>>();
        events.InitEvent<MouseMovedEvent>();

        var keyboard = new Keyboard();

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update(events);
        Assert.True(keyboard.JustPressed(Key.KeyA));
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update(events);
        Assert.True(keyboard.JustReleased(Key.KeyA));

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update(events);
        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update(events);
        Assert.True(keyboard.Pressed(Key.KeyA));
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update(events);
        Assert.True(keyboard.JustReleased(Key.KeyA));

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update(events);
        for (int i = 0; i < 100; i++)
        {
            keyboard.SetKeyState(Key.KeyA, true);
            keyboard.Update(events);
            Assert.True(keyboard.Pressed(Key.KeyA));
        }
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update(events);
        Assert.True(keyboard.JustReleased(Key.KeyA));
    }
}