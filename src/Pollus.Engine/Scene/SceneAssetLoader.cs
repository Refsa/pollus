namespace Pollus.Engine;

using Pollus.Assets;

public class SceneAssetLoader : AssetLoader<Scene>
{
    static readonly string[] extensions = [".scene", ".prefab"];
    public override string[] Extensions => extensions;

    public required SceneSerializer SceneSerializer { get; set; }

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        using var reader = SceneSerializer.GetReader(new());
        var ctx = new WorldSerializationContext() { AssetServer = context.AssetServer };
        var scene = reader.Parse(ctx, data);
        scene.Assets.UnionWith(ctx.Dependencies);

        foreach (var path in scene.Scenes.Keys)
        {
            _ = context.AssetServer.LoadAsync<Scene>(path);
        }

        context.SetAsset(scene);
    }
}
