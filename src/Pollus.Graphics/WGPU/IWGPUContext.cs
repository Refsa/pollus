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
    GPURenderPipeline CreateRenderPipeline(in RenderPipelineDescriptor descriptor) => new(this, descriptor);
    GPUPipelineLayout CreatePipelineLayout(in PipelineLayoutDescriptor descriptor) => new(this, descriptor);
    GPUShader CreateShaderModule(in ShaderModuleDescriptor descriptor) => new(this, descriptor);
    GPUBuffer CreateBuffer(in BufferDescriptor descriptor) => new(this, descriptor);
    GPUTexture CreateTexture(in TextureDescriptor descriptor) => new(this, descriptor);
    GPUTextureView CreateTextureView(GPUTexture texture, in TextureViewDescriptor descriptor) => new(this, texture, descriptor);
    GPUSampler CreateSampler(in SamplerDescriptor descriptor) => new(this, descriptor);
    GPUBindGroupLayout CreateBindGroupLayout(in BindGroupLayoutDescriptor descriptor) => new(this, descriptor);
    GPUBindGroup CreateBindGroup(in BindGroupDescriptor descriptor) => new(this, descriptor);
    GPUComputePipeline CreateComputePipeline(in ComputePipelineDescriptor descriptor) => new(this, descriptor);

    bool TryAcquireNextTextureView(in TextureViewDescriptor descriptor, out GPUTextureView textureView, out NativeHandle<TextureTag> textureHandle);
}
