namespace Pollus.Engine;

using Pollus.Engine.Platform;
using Pollus.Engine.Window;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;
using Pollus.ECS;

public interface IApplication
{
    bool IsRunning { get; }

    IWindow Window { get; }
    IWGPUContext GPUContext { get; }
    World World { get; }

    void Run();
    void Shutdown();
}

public class Application
{
    public static ApplicationBuilder Builder => CreateDefault();

    static Application()
    {
        ResourceFetch<IWGPUContext>.Register();
    }

    static ApplicationBuilder CreateDefault()
    {
        var builder = new ApplicationBuilder();
        builder.AddPlugins([
            new TimePlugin(),
            new PlatformEventsPlugin(),
            new WindowPlugin(),
        ]);
        return builder;
    }
}

public class ApplicationBuilder
{
    WorldBuilder worldBuilder = new();
    WindowOptions windowOptions = WindowOptions.Default;

    public WorldBuilder World => worldBuilder;
    public WindowOptions WindowOptions => windowOptions;

    public ApplicationBuilder WithWindowOptions(WindowOptions options)
    {
        windowOptions = options;
        return this;
    }

    public ApplicationBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
    {
        worldBuilder.AddPlugin<TPlugin>();
        return this;
    }

    public ApplicationBuilder AddPlugin<TPlugin>(TPlugin plugin) where TPlugin : IPlugin
    {
        worldBuilder.AddPlugin(plugin);
        return this;
    }

    public ApplicationBuilder AddPlugins(IPlugin[] plugin)
    {
        worldBuilder.AddPlugins(plugin);
        return this;
    }

    public ApplicationBuilder AddResource<T>(T resource)
        where T : notnull
    {
        worldBuilder.AddResource(resource);
        return this;
    }

    public ApplicationBuilder InitResource<T>()
        where T : notnull
    {
        worldBuilder.InitResource<T>();
        return this;
    }

    public ApplicationBuilder InitEvent<T>()
        where T : struct
    {
        worldBuilder.InitEvent<T>();
        return this;
    }

    public ApplicationBuilder AddSystem(StageLabel label, ISystemBuilder builder)
    {
        worldBuilder.AddSystem(label, builder);
        return this;
    }

    public ApplicationBuilder AddSystems(StageLabel stage, params ISystemBuilder[] builders)
    {
        worldBuilder.AddSystems(stage, builders);
        return this;
    }

    public ApplicationBuilder AddSystemSet<TSystemSet>()
        where TSystemSet : ISystemSet, new()
    {
        worldBuilder.AddSystemSet<TSystemSet>();
        return this;
    }

    public IApplication Build()
    {
        if (OperatingSystem.IsBrowser())
        {
            return new BrowserApplication(this);
        }
        else
        {
            return new DesktopApplication(this);
        }
    }

    public void Run()
    {
        Build().Run();
    }
}