namespace Pollus.Graphics.WGPU;

public abstract class WGPUResourceWrapper : IDisposable
{
    protected IWGPUContext context;

    bool isDisposed;

    public WGPUResourceWrapper(IWGPUContext context)
    {
        context.RegisterResource(this);
        this.context = context;
    }

    ~WGPUResourceWrapper() => Dispose();

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