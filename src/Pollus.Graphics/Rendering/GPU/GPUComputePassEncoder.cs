namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

unsafe public struct GPUComputePassEncoder : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.ComputePassEncoder* native;
    public nint Native => (nint)native;

    public GPUComputePassEncoder(IWGPUContext context, GPUCommandEncoder commandEncoder, string label)
    {
        using var labelPtr = new NativeUtf8(label);

        this.context = context;
        this.native = context.wgpu.CommandEncoderBeginComputePass(
            (Silk.NET.WebGPU.CommandEncoder*)commandEncoder.Native,
            new Silk.NET.WebGPU.ComputePassDescriptor
            {
                Label = labelPtr.Pointer,
            });
    }

    public void Dispose()
    {
        context.wgpu.ComputePassEncoderEnd(native);
        context.wgpu.ComputePassEncoderRelease(native);
    }

    public void Dispatch(uint x, uint y, uint z)
    {
        context.wgpu.ComputePassEncoderDispatchWorkgroups(native, x, y, z);
    }

    public void SetPipeline(GPUComputePipeline pipeline)
    {
        context.wgpu.ComputePassEncoderSetPipeline(native, (Silk.NET.WebGPU.ComputePipeline*)pipeline.Native);
    }

    public void SetBindGroup(uint group, GPUBindGroup bindGroup)
    {
        context.wgpu.ComputePassEncoderSetBindGroup(native, group, (Silk.NET.WebGPU.BindGroup*)bindGroup.Native, 0, null);
    }
}
