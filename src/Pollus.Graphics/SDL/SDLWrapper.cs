namespace Pollus.Graphics.SDL;

using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
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

        var displayRect = new Silk.NET.Maths.Rectangle<int>();
        var _ = Instance.GetDisplayBounds(options.TargetDisplay ?? 0, ref displayRect);

        var offset = new Vec2<int>(options.X, options.Y);
        if (options.Positioning == WindowPositioning.Centered)
        {
            offset.X = displayRect.HalfSize.X - (int)(options.Width / 2);
            offset.Y = displayRect.HalfSize.Y - (int)(options.Height / 2);
        }

        var window = Instance.CreateWindow(
            options.Title,
            displayRect.Origin.X + offset.X,
            displayRect.Origin.Y + offset.Y,
            (int)options.Width,
            (int)options.Height,
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