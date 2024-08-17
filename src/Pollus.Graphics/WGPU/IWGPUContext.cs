namespace Pollus.Graphics.WGPU;

using Pollus.Graphics.Rendering;

unsafe public interface IWGPUContext : IDisposable
{
#if BROWSER
    Emscripten.WGPUBrowser wgpu { get; }
#else
    Silk.NET.WebGPU.WebGPU wgpu { get; }
#endif

    Silk.NET.WebGPU.Surface* Surface { get; }
    Silk.NET.WebGPU.Adapter* Adapter { get; }
    Silk.NET.WebGPU.Device* Device { get; }
    Silk.NET.WebGPU.Queue* Queue { get; }
    Emscripten.WGPUSwapChain_Browser* SwapChain { get; }

    public bool IsReady { get; }
    void Setup();

    void RegisterResource(IGPUResourceWrapper resource);
    void ReleaseResource(IGPUResourceWrapper resource);

    TextureFormat GetSurfaceFormat();
    void Present();

    GPUSurfaceTexture CreateSurfaceTexture() => new(this);
    GPUCommandEncoder CreateCommandEncoder(ReadOnlySpan<char> label) => new(this, label);
    GPURenderPipeline CreateRenderPipeline(RenderPipelineDescriptor descriptor) => new(this, descriptor);
    GPUPipelineLayout CreatePipelineLayout(PipelineLayoutDescriptor descriptor) => new(this, descriptor);
    GPUShader CreateShaderModule(ShaderModuleDescriptor descriptor) => new(this, descriptor);
    GPUBuffer CreateBuffer(BufferDescriptor descriptor) => new(this, descriptor);
    GPUTexture CreateTexture(TextureDescriptor descriptor) => new(this, descriptor);
    GPUTextureView CreateTextureView(GPUTexture texture, TextureViewDescriptor descriptor) => new(this, texture, descriptor);
    GPUSampler CreateSampler(SamplerDescriptor descriptor) => new(this, descriptor);
    GPUBindGroupLayout CreateBindGroupLayout(BindGroupLayoutDescriptor descriptor) => new(this, descriptor);
    GPUBindGroup CreateBindGroup(BindGroupDescriptor descriptor) => new(this, descriptor);

    void QueuePoll() => wgpu.QueueSubmit(Queue, 0, null);
}
