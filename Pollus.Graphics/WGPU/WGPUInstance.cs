using System.Runtime.InteropServices;

namespace Pollus.Graphics.WGPU;

unsafe public class WGPUInstance : IDisposable
{
#if NET8_0_BROWSER
    internal WGPUBrowser wgpu;
#else
    internal Silk.NET.WebGPU.WebGPU wgpu;
#endif
    internal Silk.NET.WebGPU.Instance* instance;

    bool isDisposed;

    public WGPUInstance()
    {
#if NET8_0_BROWSER
        wgpu = new WGPUBrowser();
#else
        wgpu = Silk.NET.WebGPU.WebGPU.GetApi();
#endif

        var instanceDescriptor = new Silk.NET.WebGPU.InstanceDescriptor();
        instance = wgpu.CreateInstance(instanceDescriptor);
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