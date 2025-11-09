namespace Pollus.Graphics.WGPU;

using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Graphics.Platform;

unsafe public interface IWGPUContext : IDisposable
{
    public bool IsReady { get; }
    void Setup();

    void RegisterResource<TResource>(TResource resource) where TResource : IGPUResourceWrapper;
    void ReleaseResource<TResource>(TResource resource) where TResource : IGPUResourceWrapper;

    TextureFormat GetSurfaceFormat();
    void Present();
    void ResizeSurface(Vec2<uint> size);

    IWgpuBackend Backend { get; }
    NativeHandle<DeviceTag> DeviceHandle { get; }
    NativeHandle<QueueTag> QueueHandle { get; }

    GPUSurfaceTexture CreateSurfaceTexture() => new(this);
    GPUCommandEncoder CreateCommandEncoder(string label) => new(this, label);
    GPURenderPipeline CreateRenderPipeline(RenderPipelineDescriptor descriptor) => new(this, descriptor);
    GPUPipelineLayout CreatePipelineLayout(PipelineLayoutDescriptor descriptor) => new(this, descriptor);
    GPUShader CreateShaderModule(ShaderModuleDescriptor descriptor) => new(this, descriptor);
    GPUBuffer CreateBuffer(BufferDescriptor descriptor) => new(this, descriptor);
    GPUTexture CreateTexture(TextureDescriptor descriptor) => new(this, descriptor);
    GPUTextureView CreateTextureView(GPUTexture texture, TextureViewDescriptor descriptor) => new(this, texture, descriptor);
    GPUSampler CreateSampler(SamplerDescriptor descriptor) => new(this, descriptor);
    GPUBindGroupLayout CreateBindGroupLayout(BindGroupLayoutDescriptor descriptor) => new(this, descriptor);
    GPUBindGroup CreateBindGroup(BindGroupDescriptor descriptor) => new(this, descriptor);
    GPUComputePipeline CreateComputePipeline(ComputePipelineDescriptor descriptor) => new(this, descriptor);

    bool TryAcquireNextTextureView(out GPUTextureView textureView, TextureViewDescriptor descriptor);
}
