namespace Pollus.Engine.Runners;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class DesktopRunner : IAppRunner
{
    public bool IsBlocking => true;

    public void Setup(World world)
    {
        var options = world.Resources.Get<WindowOptions>();
        var window = Window.Create(options);
        var gc = new GraphicsContext();
        IWGPUContext? gpu = null;
        try
        {
            gpu = gc.CreateContext("main", window);
            gpu.Setup();

            world.Resources.Add(window);
            world.Resources.Add(gc);
            world.Resources.Add(gpu);
        }
        catch
        {
            gpu?.Dispose();
            gc.Dispose();
            window.Dispose();
            throw;
        }
    }

    public void Run(World world, Func<bool> isRunning, Action requestShutdown)
    {
        if (world.Resources.TryGet<IWindow>(out var window))
        {
            while (window.IsOpen && isRunning())
                world.Update();
        }
        else
        {
            while (isRunning())
                world.Update();
        }
    }
}
