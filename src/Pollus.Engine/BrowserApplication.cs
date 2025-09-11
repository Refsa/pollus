namespace Pollus.Engine;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Pollus.Audio;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Engine.Platform;
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
        
        Log.Info("Application shutdown");
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