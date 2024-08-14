namespace Pollus.Engine;

using Pollus.Audio;
using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Graphics;
using Pollus.Graphics.SDL;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class DesktopApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? windowContext;

    World world;

    bool isDisposed;
    bool isRunning;
    Action<IApplication>? OnSetup;
    Action<IApplication>? OnUpdate;

    public bool IsRunning => window.IsOpen && isRunning;
    public IWGPUContext GPUContext => windowContext!;
    public World World => world;
    public IWindow Window => window;

    public DesktopApplication(ApplicationBuilder builder)
    {
        window = Graphics.Windowing.Window.Create(builder.WindowOptions);
        world = builder.World;
        OnSetup = builder.OnSetup;
        OnUpdate = builder.OnUpdate;
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        window.Dispose();
        graphicsContext?.Dispose();
        world.Dispose();
    }

    public void Run()
    {
        isRunning = true;

        graphicsContext = new();
        windowContext = graphicsContext.CreateContext("main", window);
        windowContext.Setup();

        OnSetup?.Invoke(this);

        while (IsRunning)
        {
#if !BROWSER
            SDLWrapper.PollEvents();
            foreach (var @event in SDLWrapper.LatestEvents)
            {
                if (@event.Type is (uint)Silk.NET.SDL.EventType.Quit or (uint)Silk.NET.SDL.EventType.AppTerminating)
                {
                    window.Close();
                    break;
                }
            }
#endif

            OnUpdate?.Invoke(this);
        }

        Dispose();
    }
}
