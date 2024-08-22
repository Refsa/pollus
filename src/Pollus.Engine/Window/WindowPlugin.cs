namespace Pollus.Engine.Window;

using Pollus.ECS;
using Pollus.Engine.Platform;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class WindowPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.AddPlugins([
            new PlatformEventsPlugin()
        ]);

        world.Schedule.AddSystems(CoreStage.First, SystemBuilder.FnSystem("WindowUpdate",
        static (IWindow window, PlatformEvents platformEvents, IWGPUContext gpuContext) =>
        {
            foreach (var @event in platformEvents.Events)
            {
                if (@event.Type is (uint)Silk.NET.SDL.EventType.Quit or (uint)Silk.NET.SDL.EventType.AppTerminating)
                {
                    window.Close();
                    break;
                }

                switch ((Silk.NET.SDL.EventType)@event.Type)
                {
                    case Silk.NET.SDL.EventType.Windowevent:
                        var windowEvent = @event.Window;
                        if (windowEvent.Event is (int)Silk.NET.SDL.WindowEventID.Resized)
                        {
                            window.Size = new(windowEvent.Data1, windowEvent.Data2);
                            gpuContext.ResizeSurface(window.Size);
                        }
                        break;
                }
            }
        }));
    }
}