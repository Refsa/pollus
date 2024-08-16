using Pollus.Emscripten;
using Pollus.Graphics.SDL;

namespace Pollus.Engine.Platform;

public class PlatformEvents
{
    List<Silk.NET.SDL.Event> events { get; } = new();

    public IReadOnlyList<Silk.NET.SDL.Event> Events => events;

    public void ClearEvents()
    {
        events.Clear();
    }

    public void PollEvents()
    {
        var @event = new Silk.NET.SDL.Event();

#if BROWSER
        EmscriptenSDL.PumpEvents();
        while (EmscriptenSDL.PollEvent(ref @event) == 1) events.Add(@event);
#else
        SDLWrapper.Instance.PumpEvents();
        while (SDLWrapper.Instance.PollEvent(ref @event) == 1) events.Add(@event);
#endif
    }
}