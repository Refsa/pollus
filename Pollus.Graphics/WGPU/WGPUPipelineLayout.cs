namespace Pollus.Graphics.WGPU;

using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

unsafe public class WGPUPipelineLayout : WGPUResourceWrapper
{
    Silk.NET.WebGPU.PipelineLayout* native;

    public nint Native => (nint)native;

    public WGPUPipelineLayout(WGPUContext context, WGPUPipelineLayoutDescriptor descriptor) : base(context)
    {
        this.context = context;

        var label = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());
        var layouts = new BindGroupLayout*[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++)
        {
            layouts[i] = (BindGroupLayout*)descriptor.Layouts[i].Native;
        }

        fixed (byte* labelPtr = label)
        fixed (BindGroupLayout** layoutsPtr = &layouts[0])
        {
            var nativeDescriptor = new PipelineLayoutDescriptor(
                label: labelPtr,
                bindGroupLayoutCount: (uint)layouts.Length,
                bindGroupLayouts: layoutsPtr
            );
            native = context.wgpu.DeviceCreatePipelineLayout(context.device, nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.PipelineLayoutRelease(native);
    }
}

public class WGPUPipelineLayoutDescriptor
{
    public string Label { get; init; }
    public WGPUBindGroupLayout[] Layouts { get; init; }
}
