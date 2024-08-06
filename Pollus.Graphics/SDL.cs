#if !NET8_0_BROWSER
namespace Pollus.Graphics;

using SN_Sdl = Silk.NET.SDL.Sdl;
using SN_WindowFlags = Silk.NET.SDL.WindowFlags;
using SN_SdlNativeWindow = Silk.NET.SDL.SdlNativeWindow;
using SN_SdlProvider = Silk.NET.SDL.SdlProvider;
using SN_INativeWindow = Silk.NET.Core.Contexts.INativeWindow;
using SN_Event = Silk.NET.SDL.Event;
using SN_EventType = Silk.NET.SDL.EventType;
using Pollus.Utils;

public static class SDL
{
    public static readonly SN_Sdl Instance;

    static SDL()
    {
#if NET8_0_BROWSER
        Instance = new SN_Sdl(SN_Sdl.CreateDefaultContext(["SDL"]));
#else
        SN_SdlProvider.InitFlags = SN_Sdl.InitVideo | SN_Sdl.InitEvents;
        while (SN_SdlProvider.SDL.IsValueCreated is false) { }
        Instance = SN_SdlProvider.SDL.Value;
#endif
    }

    unsafe public static SN_INativeWindow CreateWindow(WindowOptions options)
    {
        using var title = TemporaryPin.PinString(options.Title);

        var window = Instance.CreateWindow(
            (byte*)title.Ptr,
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
#endif