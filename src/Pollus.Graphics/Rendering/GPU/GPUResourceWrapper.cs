namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

public interface IGPUResourceWrapper : IDisposable
{
}

public abstract class GPUResourceWrapper : IGPUResourceWrapper
{
    protected IWGPUContext context;

    bool isDisposed;
    public bool Disposed => isDisposed;

    protected GPUResourceWrapper(IWGPUContext context)
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