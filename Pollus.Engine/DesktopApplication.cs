namespace Pollus.Engine;

using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

public class DesktopApplication : IApplication, IDisposable
{
    IWindow window;
    GraphicsContext? graphicsContext;
    IWGPUContext? windowContext;

    bool isDisposed;
    Action<IApplication>? OnSetup;
    Action<IApplication>? OnUpdate;
    
    public bool IsRunning => window.IsOpen;
    public IWGPUContext WindowContext => windowContext!;

    public DesktopApplication(ApplicationBuilder builder)
    {
        window = Window.Create(builder.WindowOptions);
        OnSetup = builder.OnSetup;
        OnUpdate = builder.OnUpdate;
    }

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
        windowContext = graphicsContext.CreateContext("main", window);
        windowContext.Setup();

        OnSetup?.Invoke(this);
        window.Run(RunInternal);
        Dispose();
    }

    void RunInternal()
    {
        if (!IsRunning)
        {
            Dispose();
            return;
        }
        OnUpdate?.Invoke(this);
    }
}
