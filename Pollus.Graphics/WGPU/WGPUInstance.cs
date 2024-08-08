namespace Pollus.Graphics.WGPU;

unsafe public class WGPUInstance : IDisposable
{
#if BROWSER
    internal Browser.WGPUBrowser wgpu;
#else
    internal Silk.NET.WebGPU.WebGPU wgpu;
#endif
    internal Silk.NET.WebGPU.Instance* instance;

    bool isDisposed;

    public bool IsReady => instance != null;

    public WGPUInstance()
    {
#if BROWSER
        wgpu = new Browser.WGPUBrowser();
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