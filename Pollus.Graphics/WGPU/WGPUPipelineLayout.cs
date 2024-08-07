namespace Pollus.Graphics.WGPU;

using System.Runtime.InteropServices;

unsafe public class WGPUPipelineLayout : WGPUResourceWrapper
{
    Silk.NET.WebGPU.PipelineLayout* native;

    public nint Native => (nint)native;

    public WGPUPipelineLayout(IWGPUContext context, WGPUPipelineLayoutDescriptor descriptor) : base(context)
    {
        this.context = context;

        var label = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());
        var layouts = new Silk.NET.WebGPU.BindGroupLayout*[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++)
        {
            layouts[i] = (Silk.NET.WebGPU.BindGroupLayout*)descriptor.Layouts[i].Native;
        }

        fixed (byte* labelPtr = label)
        fixed (Silk.NET.WebGPU.BindGroupLayout** layoutsPtr = &layouts[0])
        {
            var nativeDescriptor = new Silk.NET.WebGPU.PipelineLayoutDescriptor(
                label: labelPtr,
                bindGroupLayoutCount: (uint)layouts.Length,
                bindGroupLayouts: layoutsPtr
            );
            native = context.wgpu.DeviceCreatePipelineLayout(context.Device, nativeDescriptor);
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
