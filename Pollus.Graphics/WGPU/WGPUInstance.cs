namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public class WGPUInstance : IDisposable
{
    internal WebGPU wgpu;
    internal Instance* instance;

    bool isDisposed;

    public WGPUInstance()
    {
        wgpu = WebGPU.GetApi();
        instance = wgpu.CreateInstance(null);
    }

    ~WGPUInstance() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        wgpu.InstanceRelease(instance);
        wgpu.Dispose();
    }
}