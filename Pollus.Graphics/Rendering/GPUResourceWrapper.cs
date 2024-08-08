namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

public abstract class GPUResourceWrapper : IDisposable
{
    protected IWGPUContext context;

    bool isDisposed;

    public GPUResourceWrapper(IWGPUContext context)
    {
        context.RegisterResource(this);
        this.context = context;
    }

    ~GPUResourceWrapper() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        
        context.ReleaseResource(this);
        Free();
    }

    protected abstract void Free();
}