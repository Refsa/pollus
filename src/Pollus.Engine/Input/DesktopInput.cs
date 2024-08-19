namespace Pollus.Engine.Input;

using Pollus.Engine.Platform;
using Pollus.Graphics.SDL;

public class DesktopInput : InputManager
{
    Guid keyboardId;

    public DesktopInput()
    {
        // Currently on supports a single keyboard
        {
            var keyboard = new Keyboard();
            keyboardId = keyboard.Id;
            AddDevice(keyboard);
        }
    }

    protected override void Dispose(bool disposing)
    {

    }

    protected override void UpdateInternal(PlatformEvents platform)
    {
#if !BROWSER
        var keyboard = GetDevice(keyboardId) as Keyboard;
        foreach (var @event in platform.Events)
        {
            if (@event.Type is (uint)Silk.NET.SDL.EventType.Keydown or (uint)Silk.NET.SDL.EventType.Keyup)
            {
                var key = SDLMapping.MapKey(@event.Key.Keysym.Scancode);
                if (@event.Key.Repeat == 0)
                {
                    keyboard?.SetKeyState(key, @event.Key.State == 1);
                }
            }
        }
#endif
    }
}