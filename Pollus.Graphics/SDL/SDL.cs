namespace Pollus.Graphics.SDL;

using Pollus.Graphics.Windowing;
using Silk.NET.Core.Contexts;
using Silk.NET.SDL;

public static class SDLWrapper
{
    public static readonly Sdl Instance = SdlProvider.SDL.Value;

    static List<Event> latestEvents { get; } = new();
    public static IReadOnlyList<Event> LatestEvents => latestEvents;

    static SDLWrapper()
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
        latestEvents.Clear();
        Instance.PumpEvents();

        var @event = new Event();
        while (Instance.PollEvent(ref @event) == 1)
        {
            latestEvents.Add(@event);
        }
    }
}