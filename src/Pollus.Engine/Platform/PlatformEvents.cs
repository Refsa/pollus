namespace Pollus.Engine.Platform;

using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Graphics.SDL;
using Pollus.Mathematics;

public class PlatformEvents
{
    List<Silk.NET.SDL.Event> events { get; } = new();
    Vec2<int> mousePosition = new();

    public IReadOnlyList<Silk.NET.SDL.Event> Events => events;
    public Vec2<int> MousePosition => mousePosition;

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
        EmscriptenSDL.GetMouseState(ref mousePosition.X, ref mousePosition.Y);
#else
        SDLWrapper.Instance.PumpEvents();
        while (SDLWrapper.Instance.PollEvent(ref @event) == 1) events.Add(@event);
        SDLWrapper.Instance.GetMouseState(ref mousePosition.X, ref mousePosition.Y);
#endif
    }
}

public class PlatformEventsPlugin : IPlugin
{
    static PlatformEventsPlugin()
    {
        ResourceFetch<PlatformEvents>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Add(new PlatformEvents());

        world.Schedule.AddSystems(CoreStage.First, new[]
        {
            SystemBuilder.FnSystem("PollEvents", (PlatformEvents events) =>
            {
                events.ClearEvents();
                events.PollEvents();
            })
        });
    }
}