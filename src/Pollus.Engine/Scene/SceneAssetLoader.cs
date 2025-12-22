namespace Pollus.Engine;

using Pollus.Engine.Assets;

public class SceneAssetLoader : AssetLoader<Scene>
{
    static readonly string[] extensions = [".scene", ".prefab"];
    public override string[] Extensions => extensions;

    public required SceneSerializer SceneSerializer { get; set; }

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<Scene> context)
    {
        using var reader = SceneSerializer.GetReader(new());
        var scene = reader.Parse(new() { AssetServer = context.AssetServer }, data);
        foreach (var path in scene.Scenes.Keys)
        {
            context.AssetServer.Load<Scene>(path);
        }

        context.SetAsset(scene);
    }
}