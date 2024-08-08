namespace Pollus.Engine;

using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class BrowserApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? windowContext;

    bool isDisposed;
    bool isSetup;
    Action<IApplication>? OnSetup;
    Action<IApplication>? OnUpdate;

    public bool IsRunning => window.IsOpen;
    public IWGPUContext WindowContext => windowContext!;

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