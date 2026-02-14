namespace Pollus.Platform.Runners;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class BrowserRunner : IAppRunner
{
    IWindow? window;
    IWGPUContext? gpuContext;
    Func<bool>? isRunning;
    Action? requestShutdown;
    World? world;
    bool isSetup;

    public bool IsBlocking => false;

    public void Setup(World world)
    {
        var options = world.Resources.Get<WindowOptions>();
        var window = Window.Create(options);
        var gc = new GraphicsContext();
        world.Resources.Add(window);
        world.Resources.Add(gc);
    }

    public void Run(World world, Func<bool> isRunning, Action requestShutdown)
    {
        this.world = world;
        this.isRunning = isRunning;
        this.requestShutdown = requestShutdown;

        window = world.Resources.Get<IWindow>();
        window.Run(RunInternal);
    }

    void RunInternal()
    {
        if (!window!.IsOpen)
        {
            requestShutdown!();
            return;
        }

        if (!isRunning!())
            return;

        if (!isSetup)
        {
            if (!GraphicsSetup()) return;
            isSetup = true;
            return;
        }

        world!.Update();
    }

    bool GraphicsSetup()
    {
        if (!world!.Resources.TryGet<GraphicsContext>(out var gc))
            return true;

        gpuContext ??= gc.CreateContext("main", window!);
        if (!gpuContext.IsReady)
        {
            gpuContext.Setup();
            return false;
        }
        world.Resources.Add(gpuContext);
        return true;
    }
}
