namespace Pollus.Graphics;

using SN_Sdl = Silk.NET.SDL.Sdl;
using SN_WindowFlags = Silk.NET.SDL.WindowFlags;
using SN_SdlNativeWindow = Silk.NET.SDL.SdlNativeWindow;
using SN_SdlProvider = Silk.NET.SDL.SdlProvider;
using SN_INativeWindow = Silk.NET.Core.Contexts.INativeWindow;
using SN_Event = Silk.NET.SDL.Event;
using SN_EventType = Silk.NET.SDL.EventType;

public static class SDL
{
    public static readonly SN_Sdl Instance = SN_SdlProvider.SDL.Value;

    static SDL()
    {
        SN_SdlProvider.InitFlags = SN_Sdl.InitVideo | SN_Sdl.InitEvents;
        while (SN_SdlProvider.SDL.IsValueCreated is false) { }
    }

    unsafe public static SN_INativeWindow CreateWindow(WindowOptions options)
    {
        var window = Instance.CreateWindow(
            options.Title,
            options.X,
            options.Y,
            options.Width,
            options.Height,
            (uint)SN_WindowFlags.None
        );

        return new SN_SdlNativeWindow(
            Instance, window
        );
    }

    unsafe public static void DestroyWindow(SN_INativeWindow window)
    {
        Instance.DestroyWindow((Silk.NET.SDL.Window*)window.Sdl!);
    }

    public static IEnumerable<WindowEvent> PollEvents()
    {
        Instance.PumpEvents();

        var @event = new SN_Event();
        while (Instance.PollEvent(ref @event) == 1)
        {
            switch ((SN_EventType)@event.Type)
            {
                case SN_EventType.Quit:
                case SN_EventType.AppTerminating:
                    yield return WindowEventType.Closed;
                    break;
                case SN_EventType.Windowevent:
                    break;
            }
        }
    }
}