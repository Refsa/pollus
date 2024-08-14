namespace Pollus.Tests.Input;

using Pollus.Engine.Input;

public class InputTests
{
    [Fact]
    public void Test_Keyboard()
    {
        var keyboard = new Keyboard();

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update();
        Assert.True(keyboard.JustPressed(Key.KeyA));
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update();
        Assert.True(keyboard.JustReleased(Key.KeyA));

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update();
        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update();
        Assert.True(keyboard.Pressed(Key.KeyA));
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update();
        Assert.True(keyboard.JustReleased(Key.KeyA));

        keyboard.SetKeyState(Key.KeyA, true);
        keyboard.Update();
        for (int i = 0; i < 100; i++)
        {
            keyboard.SetKeyState(Key.KeyA, true);
            keyboard.Update();
            Assert.True(keyboard.Pressed(Key.KeyA));
        }
        keyboard.SetKeyState(Key.KeyA, false);
        keyboard.Update();
        Assert.True(keyboard.JustReleased(Key.KeyA));
    }
}