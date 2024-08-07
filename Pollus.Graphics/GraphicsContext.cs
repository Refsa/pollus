namespace Pollus.Graphics;

using Pollus.Graphics.WGPU;

unsafe public class GraphicsContext : IDisposable
{
    WGPUInstance instance;
    Dictionary<string, IWGPUContext> contexts = new();

    bool isDisposed;

    public GraphicsContext()
    {
        instance = new();
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var context in contexts.Values)
        {
            context.Dispose();
        }

        instance.Dispose();
    }

    public IWGPUContext CreateContext(string name, Window window)
    {
#if NET8_0_BROWSER
        var context = new WGPUContextBrowser(window, instance);
#else
        var context = new WGPUContextDesktop(window, instance);
#endif
        contexts.Add(name, context);
        return context;
    }

    public IWGPUContext GetContext(string name)
    {
        return contexts[name];
    }
}
