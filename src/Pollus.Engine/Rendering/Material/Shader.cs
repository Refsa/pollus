namespace Pollus.Engine.Rendering;

using System.Text;
using Core.Assets;
using Pollus.Engine.Assets;

[Asset]
public partial class ShaderAsset
{
    public required string Name { get; set; }
    public required string Source { get; set; }
}

public class WgslShaderSourceLoader : AssetLoader<ShaderAsset>
{
    public override string[] Extensions => [".wgsl"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        context.SetAsset(new ShaderAsset
        {
            Name = context.FileName,
            Source = Encoding.UTF8.GetString(data)
        });
    }
}