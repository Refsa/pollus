namespace Pollus.Graphics.WGPU;

public abstract class WGPUResouceWrapper : IDisposable
{
    protected WGPUContext context;

    bool isDisposed;

    public WGPUResouceWrapper(WGPUContext context)
    {
        context.RegisterResource(this);
        this.context = context;
    }

    ~WGPUResouceWrapper() => Dispose();

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