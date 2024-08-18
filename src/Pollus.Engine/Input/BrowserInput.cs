namespace Pollus.Engine.Input;

using Pollus.Emscripten;
using Pollus.Engine.Platform;

public class BrowserInput : InputManager
{
    Guid keyboardId;

    public BrowserInput()
    {
        var sdlFlags = SDLInitFlags.InitEvents | SDLInitFlags.InitGamecontroller | SDLInitFlags.InitJoystick;
        if (EmscriptenSDL.WasInit(sdlFlags) is false)
        {
            if (EmscriptenSDL.AnyInitialized())
            {
                EmscriptenSDL.InitSubSystem(sdlFlags);
            }
            else
            {
                EmscriptenSDL.Init(sdlFlags);
            }
        }

        // Currently only supports a single keyboard
        {
            var keyboard = new Keyboard();
            keyboardId = keyboard.Id;
            AddDevice(keyboard);
        }
    }

    protected override void Dispose(bool disposing)
    {
        EmscriptenSDL.Quit();
    }

    unsafe protected override void UpdateInternal(PlatformEvents platform)
    {
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
    }
}