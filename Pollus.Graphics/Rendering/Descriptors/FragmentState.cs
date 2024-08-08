namespace Pollus.Graphics.Rendering;

public struct FragmentState
{
    public GPUShader ShaderModule;
    public string EntryPoint;

    public ConstantEntry[]? Constants;

    public ColorTargetState[]? ColorTargets;
}
