namespace Pollus.Graphics.Rendering;

using System.Text;
using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public class GPUShader : GPUResourceWrapper
{
    Silk.NET.WebGPU.ShaderModule* native;

    public nint Native => (nint)native;

    public GPUShader(IWGPUContext context, ShaderModuleDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        using var contentData = new NativeUtf8(descriptor.Content);
        
        var nativeDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor(
            label: labelData.Pointer
        );

        var shaderModuleDescriptor = descriptor.Backend switch
        {
            ShaderBackend.WGSL => new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor(
                chain: new Silk.NET.WebGPU.ChainedStruct(sType: Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor),
                code: contentData.Pointer
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
