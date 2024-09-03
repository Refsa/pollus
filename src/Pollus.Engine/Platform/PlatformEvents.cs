namespace Pollus.Engine.Platform;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Graphics.SDL;
using Pollus.Mathematics;


public class PlatformEvents
{
    List<Silk.NET.SDL.Event> events { get; } = new();

    public ListEnumerable<Silk.NET.SDL.Event> Events => new ListEnumerable<Silk.NET.SDL.Event>(events);

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

public class PlatformEventsPlugin : IPlugin
{
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