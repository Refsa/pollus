namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Utils;

unsafe public class GPUShader : GPUResourceWrapper
{
    Silk.NET.WebGPU.ShaderModule* native;

    public nint Native => (nint)native;

    public GPUShader(IWGPUContext context, ShaderModuleDescriptor descriptor) : base(context)
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

        native = context.wgpu.DeviceCreateShaderModule(context.Device, &nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.ShaderModuleRelease(native);
    }
}
