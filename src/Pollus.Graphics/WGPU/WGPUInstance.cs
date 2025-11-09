using Pollus.Debugging;

namespace Pollus.Graphics.WGPU;

using Pollus.Graphics.Platform;

unsafe public partial class WGPUInstance : IDisposable
{
    IWgpuBackend backend;
    NativeHandle<InstanceTag> instance;
    bool isDisposed;

    public bool IsReady => !instance.IsNull;
    public IWgpuBackend Backend => backend;
    public NativeHandle<InstanceTag> Instance => instance;

    public WGPUInstance()
    {
        backend = WgpuBackendProvider.Get();
        instance = backend.CreateInstance();
    }

    ~WGPUInstance() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        instance = new NativeHandle<InstanceTag>(0);
    }
}