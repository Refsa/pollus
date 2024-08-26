namespace Pollus.Graphics.WGPU;

using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

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

    void RegisterResource<TResource>(TResource resource) where TResource : IGPUResourceWrapper;
    void ReleaseResource<TResource>(TResource resource) where TResource : IGPUResourceWrapper;

    TextureFormat GetSurfaceFormat();
    void Present();
    void ResizeSurface(Vec2<int> size);

    unsafe Silk.NET.WebGPU.SurfaceTexture SurfaceGetCurrentTexture()
    {
        Silk.NET.WebGPU.SurfaceTexture surfaceTexture = new();
        wgpu.SurfaceGetCurrentTexture(Surface, ref surfaceTexture);
        return surfaceTexture;
    }

    void ReleaseSurfaceTexture(Silk.NET.WebGPU.SurfaceTexture surfaceTexture)
    {
        wgpu.TextureRelease(surfaceTexture.Texture);
    }

    GPUSurfaceTexture CreateSurfaceTexture() => new(this);
    GPUCommandEncoder CreateCommandEncoder(ReadOnlySpan<char> label) => new(this, label);
    GPURenderPipeline CreateRenderPipeline(RenderPipelineDescriptor descriptor) => new(this, descriptor);
    GPUPipelineLayout CreatePipelineLayout(PipelineLayoutDescriptor descriptor) => new(this, descriptor);
    GPUShader CreateShaderModule(ShaderModuleDescriptor descriptor) => new(this, descriptor);
    GPUBuffer CreateBuffer(BufferDescriptor descriptor) => new(this, descriptor);
    GPUTexture CreateTexture(TextureDescriptor descriptor) => new(this, descriptor);
    GPUTextureView CreateTextureView(GPUTexture texture, TextureViewDescriptor descriptor) => new(this, texture, descriptor);
    GPUTextureView CreateTextureView(Silk.NET.WebGPU.SurfaceTexture texture, TextureViewDescriptor descriptor) => new(this, texture.Texture);
    GPUSampler CreateSampler(SamplerDescriptor descriptor) => new(this, descriptor);
    GPUBindGroupLayout CreateBindGroupLayout(BindGroupLayoutDescriptor descriptor) => new(this, descriptor);
    GPUBindGroup CreateBindGroup(BindGroupDescriptor descriptor) => new(this, descriptor);

    void QueuePoll() => wgpu.QueueSubmit(Queue, 0, null);
}
