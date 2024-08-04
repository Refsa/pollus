namespace Pollus.Graphics;

using System.Diagnostics.CodeAnalysis;
using Pollus.Graphics.WGPU;
using Silk.NET.WebGPU;

unsafe public class GraphicsContext : IDisposable
{
    WGPUInstance instance;
    Dictionary<string, WGPUContext> contexts = new();

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

    public WGPUContext CreateContext(string name, Window window)
    {
        var context = new WGPUContext(window, instance);
        context.Setup();
        contexts.Add(name, context);
        return context;
    }

    public WGPUContext GetContext(string name)
    {
        return contexts[name];
    }
}
