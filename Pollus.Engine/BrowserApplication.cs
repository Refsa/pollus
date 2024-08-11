namespace Pollus.Engine;

using Pollus.Audio;
using Pollus.Engine.Input;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class BrowserApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? windowContext;

    AudioManager? audio;
    InputManager? input;

    bool isDisposed;
    bool isSetup;
    Action<IApplication>? OnSetup;
    Action<IApplication>? OnUpdate;

    public bool IsRunning => window.IsOpen;
    public IWGPUContext GPUContext => windowContext!;
    public AudioManager Audio => audio!;
    public InputManager Input => input!;

    public BrowserApplication(ApplicationBuilder builder)
    {
        window = Window.Create(builder.WindowOptions);

        OnSetup = builder.OnSetup;
        OnUpdate = builder.OnUpdate;
    }

    ~BrowserApplication() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        window.Dispose();
        graphicsContext?.Dispose();
    }

    public void Run()
    {
        graphicsContext = new();
        audio = new();
        input = new BrowserInput();
        window.Run(RunInternal);
    }

    void RunInternal()
    {
        if (!IsRunning) return;

        if (!isSetup)
        {
            if (!GraphicsSetup()) return;

            OnSetup?.Invoke(this);
            isSetup = true;
            return;
        }

        input.Update();
        OnUpdate?.Invoke(this);
    }

    bool GraphicsSetup()
    {
        windowContext ??= graphicsContext!.CreateContext("main", window);
        if (!windowContext.IsReady)
        {
            windowContext.Setup();
            return false;
        }
        return true;
    }
}