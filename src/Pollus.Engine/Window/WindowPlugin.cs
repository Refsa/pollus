namespace Pollus.Engine.Window;

using Pollus.Mathematics;
using Pollus.ECS;
using Pollus.Engine.Platform;
using Pollus.Graphics.Windowing;

public static class WindowEvent
{
    public struct Resized
    {
        public required Vec2<uint> Size { get; init; }
    }

    public struct Moved
    {
        public required Vec2<uint> Position { get; init; }
    }

    public struct Maximized;

    public struct Minimized;

    public struct FocusEntered;

    public struct FocusLeft;
}

public class WindowPlugin : IPlugin
{
    public const string WindowUpdateSystem = "Window::WindowUpdate";

    static WindowPlugin()
    {
        ResourceFetch<IWindow>.Register();
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<PlatformEventsPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Events.InitEvent<WindowEvent.Resized>();
        world.Events.InitEvent<WindowEvent.Moved>();
        world.Events.InitEvent<WindowEvent.Maximized>();
        world.Events.InitEvent<WindowEvent.Minimized>();
        world.Events.InitEvent<WindowEvent.FocusEntered>();
        world.Events.InitEvent<WindowEvent.FocusLeft>();

        world.Schedule.AddSystems(CoreStage.First, FnSystem.Create(new(WindowUpdateSystem)
            {
                RunsAfter = [PlatformEventsPlugin.PollEventsSystem],
            },
            static (IWindow window, PlatformEvents platformEvents,
                EventWriter<WindowEvent.Resized> eWindowResized,
                EventWriter<WindowEvent.Moved> eWindowMoved,
                EventWriter<WindowEvent.Maximized> eWindowMaximized,
                EventWriter<WindowEvent.Minimized> eWindowMinimized,
                EventWriter<WindowEvent.FocusEntered> eWindowFocusEntered,
                EventWriter<WindowEvent.FocusLeft> eWindowFocusLeft
            ) =>
            {
                foreach (scoped ref readonly var @event in platformEvents.Events)
                {
                    if (@event.Type is (int)Silk.NET.SDL.EventType.Quit or (int)Silk.NET.SDL.EventType.AppTerminating)
                    {
                        window.Close();
                        break;
                    }

                    switch ((Silk.NET.SDL.EventType)@event.Type)
                    {
                        case Silk.NET.SDL.EventType.Windowevent:
                            var windowEvent = @event.Window;
                            if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Resized or (int)Silk.NET.SDL.WindowEventID.SizeChanged)
                            {
                                window.Size = new((uint)windowEvent.Data1, (uint)windowEvent.Data2);
                                eWindowResized.Write(new() { Size = window.Size });
                            }
                            else if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Maximized)
                            {
                                eWindowMaximized.Write(new());
                            }
                            else if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Minimized)
                            {
                                eWindowMinimized.Write(new());
                            }
                            else if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Moved)
                            {
                                eWindowMoved.Write(new() { Position = new((uint)windowEvent.Data1, (uint)windowEvent.Data2) });
                            }
                            else if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Enter)
                            {
                                eWindowFocusEntered.Write(new());
                            }
                            else if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Leave)
                            {
                                eWindowFocusLeft.Write(new());
                            }

                            break;
                    }
                }
            }));
    }
}