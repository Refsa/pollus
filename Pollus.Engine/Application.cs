namespace Pollus.Engine;

using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;
using Pollus.Audio;
using Pollus.Engine;
using Pollus.Engine.Input;

public record class ApplicationBuilder
{
    public static ApplicationBuilder Default => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) switch
    {
        true => Browser,
        false => Desktop
    };

    public static ApplicationBuilder Desktop => new()
    {
        Application = (builder) => new DesktopApplication(builder),
        OnSetup = (app) => { },
        OnUpdate = (app) => { },
        WindowOptions = WindowOptions.Default,
    };
    public static ApplicationBuilder Browser => new()
    {
        Application = (builder) => new BrowserApplication(builder),
        OnSetup = (app) => { },
        OnUpdate = (app) => { },
        WindowOptions = WindowOptions.Default with
        {
            Title = "canvas"
        },
    };

    public required Func<ApplicationBuilder, IApplication> Application { get; set; }
    public WindowOptions WindowOptions { get; set; } = WindowOptions.Default;

    public required Action<IApplication> OnSetup { get; set; }
    public required Action<IApplication> OnUpdate { get; set; }

    public IApplication Build()
    {
        return Application(this);
    }
}

public interface IApplication
{
    bool IsRunning { get; }

    IWindow Window { get; }
    IWGPUContext GPUContext { get; }
    AudioManager Audio { get; }
    InputManager Input { get; }

    void Run();
}
