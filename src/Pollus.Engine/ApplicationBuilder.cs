namespace Pollus.Engine;

using Pollus.ECS;
using Pollus.Engine.Runners;
using Pollus.Graphics.Windowing;

public class ApplicationBuilder
{
    WorldBuilder worldBuilder = new();
    WindowOptions windowOptions = WindowOptions.Default;
    IAppRunner? runner;

    public WorldBuilder World => worldBuilder;
    public WindowOptions WindowOptions => windowOptions;

    public ApplicationBuilder WithWindowOptions(WindowOptions options)
    {
        windowOptions = options;
        return this;
    }

    public ApplicationBuilder WithRunner(IAppRunner runner)
    {
        this.runner = runner;
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
        runner ??= DefaultRunner();
        worldBuilder.AddResource(windowOptions);
        return new Application(this, runner ?? DefaultRunner());
    }

    public void Run()
    {
        Build().Run();
    }

    static IAppRunner DefaultRunner()
    {
        if (OperatingSystem.IsBrowser()) return new BrowserRunner();
        if (OperatingSystem.IsAndroid()) return new AndroidRunner();
        return new DesktopRunner();
    }
}
