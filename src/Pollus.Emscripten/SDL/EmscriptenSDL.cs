namespace Pollus.Emscripten;

using System.Runtime.InteropServices;
using Pollus.Utils;
using Silk.NET.SDL;

public class EmscriptenSDL
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
    unsafe extern static nint SDL_CreateWindow(byte* title, int x, int y, int w, int h, WindowFlags flags);

    [DllImport("SDL")]
    extern static void SDL_DestroyWindow(nint window);

    [DllImport("SDL")]
    extern static void SDL_PumpEvents();

    [DllImport("SDL")]
    unsafe extern static int SDL_PollEvent(Event* @event);

    [DllImport("SDL")]
    unsafe extern static void SDL_GetMouseState(int* x, int* y);

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

    unsafe public static void GetMouseState(ref int x, ref int y)
    {
        fixed (int* xPtr = &x, yPtr = &y)
        {
            SDL_GetMouseState(xPtr, yPtr);
        }
    }
}

[Flags]
public enum SDLInitFlags : uint
{
    None = 0u,
    InitTimer = 1u,
    InitAudio = 16u,
    InitVideo = 32u,
    InitJoystick = 512u,
    InitHaptic = 4096u,
    InitGamecontroller = 8192u,
    InitEvents = 16384u,
    InitSensor = 32768u,
    InitNoparachute = 1048576u,
}