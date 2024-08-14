namespace Pollus.Engine;

using Pollus.Audio;
using Pollus.ECS;
using Pollus.Engine.Input;
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
    Action<IApplication>? OnSetup;
    Action<IApplication>? OnUpdate;

    public bool IsRunning => window.IsOpen;
    public IWGPUContext GPUContext => windowContext!;
    public World World => world;
    public IWindow Window => window;

    public BrowserApplication(ApplicationBuilder builder)
    {
        window = Graphics.Windowing.Window.Create(builder.WindowOptions);
        world = builder.World;
        OnSetup = builder.OnSetup;
        OnUpdate = builder.OnUpdate;
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
        graphicsContext = new();
        window.Run(RunInternal);
    }

    void RunInternal()
    {
        if (!IsRunning) return;

        if (!isSetup)
        {
            if (!GraphicsSetup()) return;

            OnSetup?.Invoke(this);
            isSetup = true;
            return;
        }

        OnUpdate?.Invoke(this);
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