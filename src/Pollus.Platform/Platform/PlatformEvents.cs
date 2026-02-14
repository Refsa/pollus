namespace Pollus.Platform;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Graphics.SDL;

public class PlatformEvents
{
    ArrayList<Silk.NET.SDL.Event> events { get; } = new();
    public ReadOnlySpan<Silk.NET.SDL.Event> Events => events.AsSpan();

    public void ClearEvents()
    {
        events.Clear();
    }

    public void PollEvents()
    {
        var @event = new Silk.NET.SDL.Event();

        if (OperatingSystem.IsBrowser())
        {
            EmscriptenSDL.PumpEvents();
            while (EmscriptenSDL.PollEvent(ref @event) == 1) events.Add(@event);
        }
        else
        {
            SDLWrapper.Instance.PumpEvents();
            while (SDLWrapper.Instance.PollEvent(ref @event) == 1) events.Add(@event);
        }
    }
}

public class PlatformEventsPlugin : IPlugin
{
    public const string PollEventsSystem = "PlatformEvents::PollEvents";

    static PlatformEventsPlugin()
    {
        ResourceFetch<PlatformEvents>.Register();
    }

    public void Apply(World world)
    {
        var platformEvents = new PlatformEvents();
        world.Resources.Add(platformEvents);

        world.Schedule.AddSystems(CoreStage.First,
            FnSystem.Create(PollEventsSystem, static (PlatformEvents events) =>
            {
                events.ClearEvents();
                events.PollEvents();
            })
        );
    }
}
