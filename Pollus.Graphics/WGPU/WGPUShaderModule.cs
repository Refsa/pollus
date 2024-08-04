namespace Pollus.Graphics.WGPU;

unsafe public class WGPUShaderModule : WGPUResourceWrapper
{
    Silk.NET.WebGPU.ShaderModule* native;

    public nint Native => (nint)native;

    public WGPUShaderModule(WGPUContext context) : base(context)
    {
        qedqedqwed
    }

    protected override void Free()
    {
        context.wgpu.ShaderModuleRelease(native);
    }
}