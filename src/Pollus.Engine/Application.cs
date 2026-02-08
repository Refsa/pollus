namespace Pollus.Engine;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Platform;
using Pollus.Engine.Runners;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public interface IApplication
{
    bool IsRunning { get; }
    World World { get; }
    void Run();
    void Shutdown();
}

public class Application : IApplication, IDisposable
{
    public static ApplicationBuilder Builder => new ApplicationBuilder()
        .AddPlugins(DefaultPlugins.Full);

    public static ApplicationBuilder Headless => new ApplicationBuilder()
        .AddPlugins(DefaultPlugins.Core)
        .WithRunner(new HeadlessRunner());

    static Application()
    {
        ResourceFetch<IWGPUContext>.Register();
        ResourceFetch<Time>.Register();
        ResourceFetch<PlatformEvents>.Register();
        ResourceFetch<IWindow>.Register();
    }

    readonly IAppRunner runner;
    World world;
    bool isDisposed;
    bool isRunning;

    public bool IsRunning => isRunning;
    public World World => world;

    internal Application(ApplicationBuilder builder, IAppRunner runner)
    {
        this.runner = runner;
        world = builder.World.Build();
    }

    public void Run()
    {
        isRunning = true;
        try
        {
            runner.Setup(world);
            runner.Run(world, () => isRunning, Shutdown);
        }
        catch
        {
            Dispose();
            throw;
        }

        if (runner.IsBlocking)
        {
            isRunning = false;
            Dispose();
        }
    }

    public void Shutdown()
    {
        isRunning = false;
        if (!runner.IsBlocking) Dispose();
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        isRunning = false;
        GC.SuppressFinalize(this);
        world.Dispose();
        Log.Debug("Application shutdown");
    }
}
