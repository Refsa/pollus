namespace Pollus.Engine.Rendering;

using System.Text;
using Core.Assets;
using Pollus.Assets;
using Pollus.Utils;

[Asset]
public partial class ShaderAsset
{
    public required string Name { get; set; }
    public required string Source { get; set; }
}

public class WgslShaderSourceLoader : AssetLoader<ShaderAsset, WgslShaderSourceLoader.LoadState>
{
    public struct LoadState
    {
        public string Source { get; set; }
        public List<(string, Handle)> Dependencies { get; set; }
    }

    public override string[] Extensions => [".wgsl"];

    protected override LoadState Preprocess(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var src = Encoding.UTF8.GetString(data);
        var deps = new List<(string, Handle)>();
        foreach (var line in src.EnumerateLines())
        {
            var trimmed = line.Trim();
            if (trimmed.IsEmpty) continue;
            if (!trimmed.StartsWith("#import ")) break;

            var path = trimmed["#import ".Length..].Trim();
            if (path.Length < 2 || path[0] != '"' || path[^1] != '"')
                throw new Exception("Import line in wgsl is wrong");

            var dep = path[1..^1].ToString();
            deps.Add((line.ToString(), context.LoadDependency<ShaderAsset>(dep)));
        }

        return new LoadState()
        {
            Source = src,
            Dependencies = deps,
        };
    }

    protected override void Load(LoadState state, ref LoadContext context)
    {
        var shaderAssets = context.AssetServer.GetAssets<ShaderAsset>();
        var result = state.Source;
        foreach ((var import, var handle) in state.Dependencies)
        {
            var content = shaderAssets.Get(handle);
            if (content is null) throw new Exception("Failed to load wgsl import");
            result = result.Replace(import, content.Source);
        }

        context.SetAsset(new ShaderAsset
        {
            Name = context.FileName,
            Source = result,
        });
    }
}
