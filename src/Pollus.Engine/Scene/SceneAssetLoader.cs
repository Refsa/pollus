namespace Pollus.Engine;

using Pollus.Engine.Assets;

public class SceneAssetLoader : AssetLoader<Scene>
{
    static readonly string[] extensions = [".scene", ".prefab"];
    public override string[] Extensions => extensions;

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<Scene> context)
    {
        using var reader = new SceneReader();
        var scene = reader.Parse(new() { AssetServer = context.AssetServer }, data);
        context.SetAsset(scene);
    }
}