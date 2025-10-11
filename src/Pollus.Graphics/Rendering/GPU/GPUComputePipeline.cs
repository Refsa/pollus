namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public class GPUComputePipeline : GPUResourceWrapper
{
    Silk.NET.WebGPU.ComputePipeline* native;
    public nint Native => (nint)native;

    public GPUComputePipeline(IWGPUContext context, ComputePipelineDescriptor descriptor) : base(context)
    {
        using var label = new NativeUtf8(descriptor.Label);
        using var entryPoint = new NativeUtf8(descriptor.Compute.EntryPoint);
        var nativeDescriptor = new Silk.NET.WebGPU.ComputePipelineDescriptor(
            label: label.Pointer,
            layout: descriptor.Layout == null ? null : (Silk.NET.WebGPU.PipelineLayout*)descriptor.Layout.Native,
            compute: new(
                module: (Silk.NET.WebGPU.ShaderModule*)descriptor.Compute.Shader.Native,
                entryPoint: entryPoint.Pointer,
                constantCount: (nuint)descriptor.Compute.Constants.Length,
                constants: descriptor.Compute.Constants.Length == 0 ? null : (Silk.NET.WebGPU.ConstantEntry*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Compute.Constants))
            )
        );
        native = context.wgpu.DeviceCreateComputePipeline(context.Device, in nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.ComputePipelineRelease(native);
    }
}