namespace Pollus.Engine;

using Pollus.Audio;
using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Engine.Platform;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class BrowserApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? windowContext;

    World world;

    bool isDisposed;
    bool isSetup;

    public bool IsRunning => window.IsOpen;
    public IWGPUContext GPUContext => windowContext!;
    public World World => world;
    public IWindow Window => window;

    public BrowserApplication(Application builder)
    {
        window = Graphics.Windowing.Window.Create(builder.WindowOptions);
        world = builder.World;
    }

    ~BrowserApplication() => Dispose();

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
        world.Prepare();

        graphicsContext = new();
        world.Resources.Add(graphicsContext);
        world.Resources.Add(window);
        window.Run(RunInternal);
    }

    void RunInternal()
    {
        if (!IsRunning) return;

        if (!isSetup)
        {
            if (!GraphicsSetup()) return;

            world.Resources.Add(windowContext!);
            isSetup = true;
            return;
        }

        World.Update();
    }

    bool GraphicsSetup()
    {
        windowContext ??= graphicsContext!.CreateContext("main", window);
        if (!windowContext.IsReady)
        {
            windowContext.Setup();
            return false;
        }
        return true;
    }
}