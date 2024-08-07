namespace Pollus.Graphics.WGPU;

unsafe public interface IWGPUContext : IDisposable
{
#if BROWSER
    Browser.WGPUBrowser wgpu { get; }
#else
    Silk.NET.WebGPU.WebGPU wgpu { get; }
#endif

    Silk.NET.WebGPU.Surface* Surface { get; }
    Silk.NET.WebGPU.Adapter* Adapter { get; }
    Silk.NET.WebGPU.Device* Device { get; }
    Silk.NET.WebGPU.Queue* Queue { get; }
    Browser.WGPUSwapChain_Browser* SwapChain { get; }

    public bool IsReady { get; }
    void Setup();

    void RegisterResource(WGPUResourceWrapper resource);
    void ReleaseResource(WGPUResourceWrapper resource);

    Silk.NET.WebGPU.TextureFormat GetSurfaceFormat();
    void Present();

    WGPUSurfaceTexture CreateSurfaceTexture() => new(this);
    WGPUCommandEncoder CreateCommandEncoder(string label) => new(this, label);
    WGPURenderPipeline CreateRenderPipeline(WGPURenderPipelineDescriptor descriptor) => new(this, descriptor);
    WGPUPipelineLayout CreatePipelineLayout(WGPUPipelineLayoutDescriptor descriptor) => new(this, descriptor);
    WGPUShaderModule CreateShaderModule(WGPUShaderModuleDescriptor descriptor) => new(this, descriptor);
}
