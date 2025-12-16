namespace Pollus.Engine;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public partial class BrowserApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? gpuContext;

    World world;

    bool isDisposed;
    bool isSetup;

    public bool IsRunning => window.IsOpen;
    public IWGPUContext GPUContext => gpuContext!;
    public World World => world;
    public IWindow Window => window;

    public BrowserApplication(ApplicationBuilder builder)
    {
        window = Graphics.Windowing.Window.Create(builder.WindowOptions);
        world = builder.World.Build();
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

        Log.Debug("Application shutdown");
    }

    public void Shutdown()
    {
        window.Close();
        Dispose();
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

            world.Resources.Add(gpuContext!);
            isSetup = true;
            return;
        }

        World.Update();
    }

    bool GraphicsSetup()
    {
        gpuContext ??= graphicsContext!.CreateContext("main", window);
        if (!gpuContext.IsReady)
        {
            gpuContext.Setup();
            return false;
        }
        return true;
    }
}