namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUShader : GPUResourceWrapper
{
    NativeHandle<ShaderModuleTag> native;

    public NativeHandle<ShaderModuleTag> Native => native;

    public GPUShader(IWGPUContext context, ShaderModuleDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        using var contentData = new NativeUtf8(descriptor.Content);
        native = context.Backend.DeviceCreateShaderModule(context.DeviceHandle, descriptor.Backend, labelData, contentData);
    }

    protected override void Free()
    {
        context.Backend.ShaderModuleRelease(native);
    }
}
