namespace Pollus.ECS;

using System.Net.Http.Headers;
using Microsoft.VisualBasic;
using Pollus.Graphics;
using Silk.NET.Core.Contexts;
using Silk.NET.SDL;

public static class SDL
{
    public static readonly Sdl Instance = SdlProvider.SDL.Value;

    static SDL()
    {
        SdlProvider.InitFlags = Sdl.InitVideo | Sdl.InitEvents;
        while (SdlProvider.SDL.IsValueCreated is false) { }
    }

    unsafe public static INativeWindow CreateWindow(WindowOptions options)
    {
        var window = Instance.CreateWindow(
            options.Title,
            options.X,
            options.Y,
            options.Width,
            options.Height,
            (uint)WindowFlags.None
        );

        return new SdlNativeWindow(
            Instance, window
        );
    }

    unsafe public static void DestroyWindow(INativeWindow window)
    {
        Instance.DestroyWindow((Silk.NET.SDL.Window*)window.Sdl!);
    }

    public static void PollEvents()
    {
        Instance.PumpEvents();

        var @event = new Event();
        while (Instance.PollEvent(ref @event) == 1)
        {
            switch ((EventType)@event.Type)
            {
                case EventType.Quit:
                case EventType.AppTerminating:
                    break;
                case EventType.Windowevent:
                    break;
            }
        }
    }
}