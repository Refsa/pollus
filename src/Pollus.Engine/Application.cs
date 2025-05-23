namespace Pollus.Engine;

using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Engine.Platform;
using Pollus.Engine.Window;

public interface IApplication
{
    bool IsRunning { get; }

    IWindow Window { get; }
    IWGPUContext GPUContext { get; }
    World World { get; }

    void Run();
    void Shutdown();
}

public record class Application
{
    public static Application Builder => new();
    static Application()
    {
        ResourceFetch<IWGPUContext>.Register();
        ResourceFetch<IWindow>.Register();
        ResourceFetch<GraphicsContext>.Register();
    }

    Func<Application, IApplication> application = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) switch
    {
        true => (builder) => new BrowserApplication(builder),
        false => (builder) => new DesktopApplication(builder)
    };

    WindowOptions windowOptions = WindowOptions.Default;
    World world = new World()
        .AddPlugins([
            new TimePlugin(),
            new PlatformEventsPlugin(),
            new WindowPlugin(),
        ]);

    public World World => world;
    public WindowOptions WindowOptions => windowOptions;

    public Application WithWindowOptions(WindowOptions options)
    {
        windowOptions = options;
        return this;
    }

    public Application AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
    {
        world.AddPlugin<TPlugin>();
        return this;
    }

    public Application AddPlugin<TPlugin>(TPlugin plugin) where TPlugin : IPlugin
    {
        world.AddPlugin(plugin);
        return this;
    }

    public Application AddPlugins(IPlugin[] plugin)
    {
        world.AddPlugins(plugin);
        return this;
    }

    public Application AddResource<T>(T resource)
        where T : notnull
    {
        world.Resources.Add(resource);
        return this;
    }

    public Application InitResource<T>()
        where T : notnull
    {
        world.Resources.Init<T>();
        return this;
    }

    public Application InitEvent<T>()
        where T : struct
    {
        world.Events.InitEvent<T>();
        return this;
    }

    public Application AddSystem(StageLabel stage, params ISystem[] systems)
    {
        world.Schedule.AddSystems(stage, systems);
        return this;
    }

    public Application AddSystem(StageLabel stage, params ISystemBuilder[] builders)
    {
        world.Schedule.AddSystems(stage, builders);
        return this;
    }

    public Application AddSystemSet<TSystemSet>() 
        where TSystemSet : ISystemSet, new()
    {
        TSystemSet.AddToSchedule(world.Schedule);
        return this;
    }

    public IApplication Build()
    {
        return application(this);
    }

    public void Run()
    {
        Build().Run();
    }
}
