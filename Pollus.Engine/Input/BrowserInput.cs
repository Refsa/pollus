namespace Pollus.Engine.Input;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

public class BrowserInput : Input
{
    Guid keyboardId;

    public BrowserInput()
    {
        var sdlFlags = (uint)(Silk.NET.SDL.Sdl.InitEvents | Silk.NET.SDL.Sdl.InitGamecontroller | Silk.NET.SDL.Sdl.InitJoystick);
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

        var window = EmscriptenSDL.CreateWindow("canvas", 
            Silk.NET.SDL.Sdl.WindowposUndefined, Silk.NET.SDL.Sdl.WindowposUndefined, 
            1600, 900, 
            Silk.NET.SDL.WindowFlags.InputFocus | Silk.NET.SDL.WindowFlags.Resizable);

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

    unsafe protected override void UpdateInternal()
    {
        var keyboard = GetDevice(keyboardId) as Keyboard;
        EmscriptenSDL.PumpEvents();

        var @event = new Silk.NET.SDL.Event();
        while (EmscriptenSDL.PollEvent(ref @event) == 1)
        {
            Console.WriteLine(@event.Type);

            if (@event.Type is (uint)Silk.NET.SDL.EventType.Keydown or (uint)Silk.NET.SDL.EventType.Keyup)
            {
                var key = MapKey(@event.Key.Keysym.Scancode);
                if (@event.Key.Repeat == 0)
                {
                    keyboard?.SetKeyState(key, @event.Key.State == 1);
                }
            }
        }
    }

    Key MapKey(Silk.NET.SDL.Scancode scancode)
    {
        return scancode switch
        {
            Silk.NET.SDL.Scancode.ScancodeA => Key.KeyA,
            Silk.NET.SDL.Scancode.ScancodeB => Key.KeyB,
            Silk.NET.SDL.Scancode.ScancodeC => Key.KeyC,
            Silk.NET.SDL.Scancode.ScancodeD => Key.KeyD,
            Silk.NET.SDL.Scancode.ScancodeE => Key.KeyE,
            Silk.NET.SDL.Scancode.ScancodeF => Key.KeyF,
            Silk.NET.SDL.Scancode.ScancodeG => Key.KeyG,
            Silk.NET.SDL.Scancode.ScancodeH => Key.KeyH,
            Silk.NET.SDL.Scancode.ScancodeI => Key.KeyI,
            Silk.NET.SDL.Scancode.ScancodeJ => Key.KeyJ,
            Silk.NET.SDL.Scancode.ScancodeK => Key.KeyK,
            Silk.NET.SDL.Scancode.ScancodeL => Key.KeyL,
            Silk.NET.SDL.Scancode.ScancodeM => Key.KeyM,
            Silk.NET.SDL.Scancode.ScancodeN => Key.KeyN,
            Silk.NET.SDL.Scancode.ScancodeO => Key.KeyO,
            Silk.NET.SDL.Scancode.ScancodeP => Key.KeyP,
            Silk.NET.SDL.Scancode.ScancodeQ => Key.KeyQ,
            Silk.NET.SDL.Scancode.ScancodeR => Key.KeyR,
            Silk.NET.SDL.Scancode.ScancodeS => Key.KeyS,
            Silk.NET.SDL.Scancode.ScancodeT => Key.KeyT,
            Silk.NET.SDL.Scancode.ScancodeU => Key.KeyU,
            Silk.NET.SDL.Scancode.ScancodeV => Key.KeyV,
            Silk.NET.SDL.Scancode.ScancodeW => Key.KeyW,
            Silk.NET.SDL.Scancode.ScancodeX => Key.KeyX,
            Silk.NET.SDL.Scancode.ScancodeY => Key.KeyY,
            Silk.NET.SDL.Scancode.ScancodeZ => Key.KeyZ,
            Silk.NET.SDL.Scancode.Scancode1 => Key.Digit1,
            Silk.NET.SDL.Scancode.Scancode2 => Key.Digit2,
            Silk.NET.SDL.Scancode.Scancode3 => Key.Digit3,
            Silk.NET.SDL.Scancode.Scancode4 => Key.Digit4,
            Silk.NET.SDL.Scancode.Scancode5 => Key.Digit5,
            Silk.NET.SDL.Scancode.Scancode6 => Key.Digit6,
            Silk.NET.SDL.Scancode.Scancode7 => Key.Digit7,
            Silk.NET.SDL.Scancode.Scancode8 => Key.Digit8,
            Silk.NET.SDL.Scancode.Scancode9 => Key.Digit9,
            Silk.NET.SDL.Scancode.Scancode0 => Key.Digit0,
            Silk.NET.SDL.Scancode.ScancodeReturn => Key.Enter,
            Silk.NET.SDL.Scancode.ScancodeEscape => Key.Escape,
            Silk.NET.SDL.Scancode.ScancodeBackspace => Key.Backspace,
            Silk.NET.SDL.Scancode.ScancodeTab => Key.Tab,
            Silk.NET.SDL.Scancode.ScancodeSpace => Key.Space,
            Silk.NET.SDL.Scancode.ScancodeMinus => Key.Minus,
            Silk.NET.SDL.Scancode.ScancodeEquals => Key.Equal,
            Silk.NET.SDL.Scancode.ScancodeLeftbracket => Key.LeftBracket,
            Silk.NET.SDL.Scancode.ScancodeRightbracket => Key.RightBracket,
            Silk.NET.SDL.Scancode.ScancodeBackslash => Key.Backslash,
            Silk.NET.SDL.Scancode.ScancodeNonushash => Key.NonUSHash,
            Silk.NET.SDL.Scancode.ScancodeSemicolon => Key.Semicolon,
            Silk.NET.SDL.Scancode.ScancodeApostrophe => Key.Apostrophe,
            Silk.NET.SDL.Scancode.ScancodeGrave => Key.Grave,
            Silk.NET.SDL.Scancode.ScancodeComma => Key.Comma,
            Silk.NET.SDL.Scancode.ScancodePeriod => Key.Period,
            Silk.NET.SDL.Scancode.ScancodeSlash => Key.Slash,
            Silk.NET.SDL.Scancode.ScancodeCapslock => Key.CapsLock,
            Silk.NET.SDL.Scancode.ScancodeF1 => Key.F1,
            Silk.NET.SDL.Scancode.ScancodeF2 => Key.F2,
            Silk.NET.SDL.Scancode.ScancodeF3 => Key.F3,
            Silk.NET.SDL.Scancode.ScancodeF4 => Key.F4,
            Silk.NET.SDL.Scancode.ScancodeF5 => Key.F5,
            Silk.NET.SDL.Scancode.ScancodeF6 => Key.F6,
            Silk.NET.SDL.Scancode.ScancodeF7 => Key.F7,
            Silk.NET.SDL.Scancode.ScancodeF8 => Key.F8,
            Silk.NET.SDL.Scancode.ScancodeF9 => Key.F9,
            Silk.NET.SDL.Scancode.ScancodeF10 => Key.F10,
            Silk.NET.SDL.Scancode.ScancodeF11 => Key.F11,
            Silk.NET.SDL.Scancode.ScancodeF12 => Key.F12,
            Silk.NET.SDL.Scancode.ScancodePrintscreen => Key.PrintScreen,
            Silk.NET.SDL.Scancode.ScancodeScrolllock => Key.ScrollLock,
            Silk.NET.SDL.Scancode.ScancodePause => Key.Pause,
            Silk.NET.SDL.Scancode.ScancodeInsert => Key.Insert,
            Silk.NET.SDL.Scancode.ScancodeHome => Key.Home,
            Silk.NET.SDL.Scancode.ScancodePageup => Key.PageUp,
            Silk.NET.SDL.Scancode.ScancodeDelete => Key.Delete,
            Silk.NET.SDL.Scancode.ScancodeEnd => Key.End,
            Silk.NET.SDL.Scancode.ScancodePagedown => Key.PageDown,
            Silk.NET.SDL.Scancode.ScancodeRight => Key.ArrowRight,
            Silk.NET.SDL.Scancode.ScancodeLeft => Key.ArrowLeft,
            Silk.NET.SDL.Scancode.ScancodeDown => Key.ArrowDown,
            Silk.NET.SDL.Scancode.ScancodeUp => Key.ArrowUp,
            _ => Key.Unknown,
        };
    }
}

class EmscriptenSDL
{
    [DllImport("SDL")]
    extern static int SDL_Init(uint flags);

    [DllImport("SDL")]
    extern static int SDL_InitSubSystem(uint flags);

    [DllImport("SDL")]
    extern static uint SDL_WasInit(uint flags);

    [DllImport("SDL")]
    extern static void SDL_Quit();

    [DllImport("SDL")]
    extern static void SDL_QuitSubSystem(uint flags);

    [DllImport("SDL")]
    unsafe extern static nint SDL_CreateWindow(byte* title, int x, int y, int w, int h, Silk.NET.SDL.WindowFlags flags);

    [DllImport("SDL")]
    extern static void SDL_PumpEvents();

    [DllImport("SDL")]
    unsafe extern static int SDL_PollEvent(Silk.NET.SDL.Event* @event);

    public static void Init(uint flags)
    {
        var status = SDL_Init(flags);
        if (status != 0)
        {
            throw new Exception($"SDL_Init failed with error code {status}");
        }
        else
        {
            Console.WriteLine("SDL_Init succeeded");
        }
    }

    public static void InitSubSystem(uint flags)
    {
        var status = SDL_InitSubSystem(flags);
        if (status != 0)
        {
            throw new Exception($"SDL_InitSubSystem failed with error code {status}");
        }
    }

    public static bool WasInit(uint flags)
    {
        var initializedFlags = SDL_WasInit(flags);
        return (initializedFlags & flags) == flags;
    }

    public static bool AnyInitialized()
    {
        return SDL_WasInit(~0u) != 0;
    }

    public static void Quit()
    {
        SDL_Quit();
    }

    public static void QuitSubSystem(uint flags)
    {
        SDL_QuitSubSystem(flags);
    }

    unsafe public static nint CreateWindow(string title, int x, int y, int w, int h, Silk.NET.SDL.WindowFlags flags)
    {
        var titlePtr = TemporaryPin.PinString(title);
        return SDL_CreateWindow((byte*)titlePtr.Ptr, x, y, w, h, flags);
    }

    public static void PumpEvents()
    {
        SDL_PumpEvents();
    }

    unsafe public static int PollEvent(ref Silk.NET.SDL.Event @event)
    {
        fixed (Silk.NET.SDL.Event* ptr = &@event)
        {
            return SDL_PollEvent(ptr);
        }
    }
}