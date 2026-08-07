namespace Pollus.Platform.Runners;

using System.Diagnostics;
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
        var options = world.Resources.Get<WindowOptions>();
        var frequency = Stopwatch.Frequency;
        var frameTicks = frequency / options.FramesPerSecond;
        var nextFrame = Stopwatch.GetTimestamp();

        if (world.Resources.TryGet<IWindow>(out var window))
        {
            while (window.IsOpen && isRunning())
            {
                world.Update();
                nextFrame += frameTicks;

                while (true)
                {
                    var remaining = nextFrame - Stopwatch.GetTimestamp();
                    if (remaining <= 0)
                        break;

                    var remainingMs = remaining * 1000.0 / frequency;
                    if (remainingMs > 2.0)
                        Thread.Sleep(1);
                    else
                        Thread.SpinWait(32);
                }

                if (Stopwatch.GetTimestamp() - nextFrame > frameTicks)
                    nextFrame = Stopwatch.GetTimestamp();
            }
        }
        else
        {
            while (isRunning())
                world.Update();
        }
    }
}
