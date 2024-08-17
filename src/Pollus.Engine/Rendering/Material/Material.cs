namespace Pollus.Engine.Rendering;

using System.Text;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;

public class Material : IMaterial
{
    public static string Name => "DefaultMaterial";
    public static string ShaderPath => "shaders/quad.wgsl";
    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x3,
            VertexFormat.Float32x2,
        ]),
        VertexBufferLayout.Instance(5, [
            VertexFormat.Mat4x4,
        ]),
    ];
    /* public static BindGroupLayoutEntry[][] BindGroupLayouts => [[
        BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex, false),
        BindGroupLayoutEntry.TextureEntry(1, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
        BindGroupLayoutEntry.SamplerEntry(2, ShaderStage.Fragment, SamplerBindingType.Filtering),
    ]]; */

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required IBinding[][] Bindings { get; set; }
}

public interface IMaterial
{
    public static abstract string Name { get; }

    public static abstract string ShaderPath { get; }
    public static virtual string VertexEntryPoint => "vs_main";
    public static virtual string FragmentEntryPoint => "fs_main";

    public static abstract VertexBufferLayout[] VertexLayouts { get; }
    // public static abstract BindGroupLayoutEntry[][] BindGroupLayouts { get; }

    Handle<ShaderAsset> ShaderSource { get; set; }
    IBinding[][] Bindings { get; set; }
}

public enum BindingType
{
    Uniform,
    Texture,
    Sampler,
}

public interface IBinding
{
    BindingType Type { get; }
    BindGroupLayoutEntry Layout { get; }
    ShaderStage Visibility { get; }
}

public class UniformBinding<T> : IBinding
    where T : unmanaged
{
    public BindingType Type => BindingType.Uniform;
    public BindGroupLayoutEntry Layout => BindGroupLayoutEntry.Uniform<T>(0, Visibility, false);

    public required Uniform<T> Value { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Vertex | ShaderStage.Fragment;
}

public class TextureBinding : IBinding
{
    public BindingType Type => BindingType.Texture;
    public BindGroupLayoutEntry Layout => BindGroupLayoutEntry.TextureEntry(0, Visibility, TextureSampleType.Float, TextureViewDimension.Dimension2D);

    public required Handle<ImageAsset> Image { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Fragment;
}

public class SamplerBinding : IBinding
{
    public BindingType Type => BindingType.Sampler;
    public BindGroupLayoutEntry Layout => BindGroupLayoutEntry.SamplerEntry(0, Visibility, SamplerBindingType.Filtering);

    public required Handle<ImageAsset> Sampler { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Fragment;
}