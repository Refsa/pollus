namespace Pollus.Graphics.SDL;

using Pollus.Graphics.Windowing;
using Silk.NET.Core.Contexts;
using Silk.NET.SDL;

public static class SDLWrapper
{
    public static readonly Sdl Instance = SdlProvider.SDL.Value;

    static SDLWrapper()
    {
        SdlProvider.InitFlags = Sdl.InitVideo | Sdl.InitEvents | Sdl.InitJoystick | Sdl.InitGamecontroller | Sdl.InitHaptic | Sdl.InitSensor;
        while (SdlProvider.SDL.IsValueCreated is false) { }
    }

    unsafe public static INativeWindow CreateWindow(WindowOptions options)
    {
        var flags = WindowFlags.None;
        if (options.Resizeable) flags |= WindowFlags.Resizable;
        if (options.Fullscreen) flags |= WindowFlags.Fullscreen;
        if (options.Borderless) flags |= WindowFlags.Borderless;
        if (options.MouseCapture) flags |= WindowFlags.InputGrabbed;

        var window = Instance.CreateWindow(
            options.Title,
            options.X,
            options.Y,
            options.Width,
            options.Height,
            (uint)flags
        );

        return new SdlNativeWindow(
            Instance, window
        );
    }

    unsafe public static void DestroyWindow(INativeWindow window)
    {
        Instance.DestroyWindow((Silk.NET.SDL.Window*)window.Sdl!);
    }
}