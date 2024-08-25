namespace Pollus.Emscripten;

using System.Runtime.InteropServices;
using Pollus.Utils;
using Silk.NET.SDL;

public partial class EmscriptenSDL
{
    [LibraryImport("SDL")]
    private static partial int SDL_Init(uint flags);

    [LibraryImport("SDL")]
    private static partial int SDL_InitSubSystem(uint flags);

    [LibraryImport("SDL")]
    private static partial uint SDL_WasInit(uint flags);

    [LibraryImport("SDL")]
    private static partial void SDL_Quit();

    [LibraryImport("SDL")]
    private static partial void SDL_QuitSubSystem(uint flags);

    [LibraryImport("SDL")]
    private unsafe static partial nint SDL_CreateWindow(byte* title, int x, int y, int w, int h, WindowFlags flags);

    [LibraryImport("SDL")]
    private static partial void SDL_DestroyWindow(nint window);

    [LibraryImport("SDL")]
    private static partial void SDL_PumpEvents();

    [LibraryImport("SDL")]
    private unsafe static partial int SDL_PollEvent(Event* @event);

    [LibraryImport("SDL")]
    private unsafe static partial GameController* SDL_GameControllerOpen(int index);

    [LibraryImport("SDL")]
    private unsafe static partial void SDL_GameControllerClose(GameController* gameController);

    [LibraryImport("SDL")]
    private static partial void SDL_GameControllerUpdate();

    [LibraryImport("SDL")]
    private static partial int SDL_GameControllerEventState(int state);

    [LibraryImport("SDL")]
    private static partial byte SDL_IsGameController(int index);

    [LibraryImport("SDL")]
    private static partial int SDL_JoystickOpen(int index);

    [LibraryImport("SDL")]
    private static partial void SDL_JoystickClose(nint joystick);

    [LibraryImport("SDL")]
    private static partial void SDL_JoystickUpdate();

    [LibraryImport("SDL")]
    private static partial int SDL_JoystickEventState(int state);

    [LibraryImport("SDL")]
    private static partial int SDL_NumJoysticks();

    [LibraryImport("SDL")]
    private static partial byte SDL_JoystickGetButton(nint joystick, int button);

    [LibraryImport("SDL")]
    private static partial short SDL_JoystickGetAxis(nint joystick, int axis);

    [LibraryImport("SDL")]
    [return: MarshalAs(UnmanagedType.LPUTF8Str)]
    private static partial string SDL_JoystickName(int deviceIndex);

    public static void Init(SDLInitFlags flags)
    {
        var status = SDL_Init((uint)flags);
        if (status != 0)
        {
            throw new Exception($"SDL_Init failed with error code {status}");
        }
    }

    public static void InitSubSystem(SDLInitFlags flags)
    {
        var status = SDL_InitSubSystem((uint)flags);
        if (status != 0)
        {
            throw new Exception($"SDL_InitSubSystem failed with error code {status}");
        }
    }

    public static bool WasInit(SDLInitFlags flags)
    {
        var initializedFlags = SDL_WasInit((uint)flags);
        return (initializedFlags & (uint)flags) == (uint)flags;
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

    unsafe public static nint CreateWindow(string title, int x, int y, int w, int h, WindowFlags flags)
    {
        var titlePtr = TemporaryPin.PinString(title);
        return SDL_CreateWindow((byte*)titlePtr.Ptr, x, y, w, h, flags);
    }

    public static void DestroyWindow(nint window)
    {
        SDL_DestroyWindow(window);
    }

    public static void PumpEvents()
    {
        SDL_PumpEvents();
    }

    unsafe public static int PollEvent(ref Event @event)
    {
        fixed (Event* ptr = &@event)
        {
            return SDL_PollEvent(ptr);
        }
    }

    unsafe public static nint GameControllerOpen(int index)
    {
        return (nint)SDL_GameControllerOpen(index);
    }

    unsafe public static void GameControllerClose(nint gameController)
    {
        SDL_GameControllerClose((GameController*)gameController);
    }

    public static void GameControllerUpdate()
    {
        SDL_GameControllerUpdate();
    }

    public static int GameControllerEventState(int state)
    {
        return SDL_GameControllerEventState(state);
    }

    unsafe public static nint JoystickOpen(int index)
    {
        return SDL_JoystickOpen(index);
    }

    unsafe public static void JoystickClose(nint joystick)
    {
        SDL_JoystickClose(joystick);
    }

    public static void JoystickUpdate()
    {
        SDL_JoystickUpdate();
    }

    public static void JoystickEventState(int state)
    {
        SDL_JoystickEventState(state);
    }

    public static byte JoystickGetButton(nint joystick, int button)
    {
        return SDL_JoystickGetButton(joystick, button);
    }

    public static short JoystickGetAxis(nint joystick, int axis)
    {
        return SDL_JoystickGetAxis(joystick, axis);
    }

    public static int NumJoysticks()
    {
        return SDL_NumJoysticks();
    }

    public static string JoystickName(int deviceIndex)
    {
        return SDL_JoystickName(deviceIndex);
    }

    public static bool IsGameController(int index)
    {
        return SDL_IsGameController(index) != 0;
    }
}

[Flags]
public enum SDLInitFlags
{
    None = 0,
    InitTimer = 0x00000001,
    InitAudio = 0x00000010,
    InitVideo = 0x00000020,
    InitJoystick = 0x00000200,
    InitHaptic = 0x00001000,
    InitNoparachute = 0x00100000,
    Everthing = 0x0000FFFF,
}