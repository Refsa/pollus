namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;

unsafe public class GPUShader : GPUResourceWrapper
{
    Silk.NET.WebGPU.ShaderModule* native;

    public nint Native => (nint)native;

    public GPUShader(IWGPUContext context, ShaderModuleDescriptor descriptor) : base(context)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor(
            label: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Label))
        );

        var shaderModuleDescriptor = descriptor.Backend switch
        {
            ShaderBackend.WGSL => new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor(
                chain: new Silk.NET.WebGPU.ChainedStruct(sType: Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor),
                code: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Content))
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
