namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;

unsafe public class GPUPipelineLayout : GPUResourceWrapper
{
    Silk.NET.WebGPU.PipelineLayout* native;

    public nint Native => (nint)native;

    public GPUPipelineLayout(IWGPUContext context, PipelineLayoutDescriptor descriptor) : base(context)
    {
        this.context = context;

        var layouts = stackalloc Silk.NET.WebGPU.BindGroupLayout*[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++)
        {
            layouts[i] = (Silk.NET.WebGPU.BindGroupLayout*)descriptor.Layouts[i].Native;
        }

        var nativeDescriptor = new Silk.NET.WebGPU.PipelineLayoutDescriptor(
            label: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Label)),
            bindGroupLayoutCount: (uint)descriptor.Layouts.Length,
            bindGroupLayouts: layouts
        );
        native = context.wgpu.DeviceCreatePipelineLayout(context.Device, nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.PipelineLayoutRelease(native);
    }
}
