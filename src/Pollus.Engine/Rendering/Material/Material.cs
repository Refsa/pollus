namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public interface IMaterial
{
    public static abstract string Name { get; }
    public static abstract VertexBufferLayout[] VertexLayouts { get; }
    public static abstract RenderPipelineDescriptor PipelineDescriptor { get; }
    public static virtual BlendState? Blend { get; } = null;

    Handle<ShaderAsset> ShaderSource { get; set; }
    IBinding[][] Bindings { get; }
}
