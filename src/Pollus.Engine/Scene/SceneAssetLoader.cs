namespace Pollus.Engine;

using Pollus.Assets;

public class SceneAssetLoader : AssetLoader<Scene, SceneAssetLoader.LoadState>
{
    public struct LoadState
    {
        public Scene Scene { get; set; }
    }

    static readonly string[] extensions = [".scene", ".prefab"];
    public override string[] Extensions => extensions;

    public required SceneSerializer SceneSerializer { get; set; }

    protected override LoadState Preprocess(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        using var reader = SceneSerializer.GetReader(new());
        var ctx = new WorldSerializationContext() { AssetServer = context.AssetServer };
        var scene = reader.Parse(ctx, data);
        scene.Assets.UnionWith(ctx.Dependencies);

        foreach (var path in scene.Scenes.Keys)
        {
            _ = context.LoadDependency<Scene>(path);
        }

        return new()
        {
            Scene = scene,
        };
    }

    protected override void Load(LoadState state, ref LoadContext context)
    {
        context.SetAsset(state.Scene);
    }
}
