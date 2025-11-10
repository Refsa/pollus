namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public struct GPUComputePassEncoder : IDisposable
{
    IWGPUContext context;
    NativeHandle<ComputePassEncoderTag> native;
    public NativeHandle<ComputePassEncoderTag> Native => native;

    public GPUComputePassEncoder(IWGPUContext context, GPUCommandEncoder commandEncoder, string label)
    {
        this.context = context;
        using var labelPtr = new NativeUtf8(label);
        native = context.Backend.CommandEncoderBeginComputePass(commandEncoder.Native, labelPtr);
    }

    public void Dispose()
    {
        context.Backend.ComputePassEncoderEnd(native);
    }

    public void Dispatch(uint x, uint y, uint z)
    {
        context.Backend.ComputePassEncoderDispatchWorkgroups(native, x, y, z);
    }

    public void SetPipeline(GPUComputePipeline pipeline)
    {
        context.Backend.ComputePassEncoderSetPipeline(native, pipeline.Native);
    }

    public void SetBindGroup(uint group, GPUBindGroup bindGroup)
    {
        context.Backend.ComputePassEncoderSetBindGroup(native, group, bindGroup.Native, ReadOnlySpan<uint>.Empty);
    }
}
