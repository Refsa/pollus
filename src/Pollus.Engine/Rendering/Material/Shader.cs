namespace Pollus.Engine.Rendering;

using System.Text;
using Pollus.Engine.Assets;

public class ShaderAsset
{
    public required string Source { get; set; }
}

public class WgslShaderSourceLoader : AssetLoader<ShaderAsset>
{
    public override string[] Extensions => [".wgsl"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<ShaderAsset> context)
    {
        context.SetAsset(new ShaderAsset
        {
            Source = Encoding.UTF8.GetString(data)
        });
    }
}