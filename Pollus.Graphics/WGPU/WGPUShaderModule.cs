namespace Pollus.Graphics.WGPU;

using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Utils;

unsafe public class WGPUShaderModule : WGPUResourceWrapper
{
    Silk.NET.WebGPU.ShaderModule* native;

    public nint Native => (nint)native;

    public WGPUShaderModule(WGPUContext context, WGPUShaderModuleDescriptor descriptor) : base(context)
    {
        using var labelPin = TemporaryPin.PinString(descriptor.Label);
        using var contentPin = TemporaryPin.PinString(descriptor.Content);

        var nativeDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor(
            label: (byte*)labelPin.Ptr
        );

        var shaderModuleDescriptor = descriptor.Backend switch
        {
            ShaderBackend.WGSL => new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor(
                chain: new Silk.NET.WebGPU.ChainedStruct(sType: Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor),
                code: (byte*)contentPin.Ptr
            ),
            _ => throw new NotImplementedException(),
        };
        nativeDescriptor.NextInChain = (Silk.NET.WebGPU.ChainedStruct*)&shaderModuleDescriptor;

        native = context.wgpu.DeviceCreateShaderModule(context.device, &nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.ShaderModuleRelease(native);
    }
}

public enum ShaderBackend
{
    WGSL,
}

public class WGPUShaderModuleDescriptor
{
    public ShaderBackend Backend { get; init; }
    public string Label { get; init; }
    public string Content { get; init; }
}
