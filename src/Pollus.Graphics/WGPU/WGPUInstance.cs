using Pollus.Debugging;

namespace Pollus.Graphics.WGPU;

unsafe public class WGPUInstance : IDisposable
{
#if BROWSER
    internal Emscripten.WGPUBrowser wgpu;
    internal Emscripten.WGPU.WGPUInstance* instance;
#else
    internal Silk.NET.WebGPU.WebGPU wgpu;
    internal Silk.NET.WebGPU.Instance* instance;
#endif

    bool isDisposed;

    public bool IsReady => instance != null;

    public WGPUInstance()
    {
#if BROWSER
        wgpu = new Emscripten.WGPUBrowser();
        instance = wgpu.CreateInstance(null);
#else
        wgpu = Silk.NET.WebGPU.WebGPU.GetApi();
        var instanceDescriptor = new Silk.NET.WebGPU.InstanceDescriptor();
        instance = wgpu.CreateInstance(ref instanceDescriptor);
#endif

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