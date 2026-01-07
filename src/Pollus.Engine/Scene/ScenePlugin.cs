namespace Pollus.Engine;

using Pollus.Engine.Assets;
using Pollus.ECS;
using Utils;
using System.Runtime.CompilerServices;

public partial struct SceneRoot : IComponent
{
}

public partial struct SceneRef : IComponent
{
    public required Handle<Scene> Scene;
}

public partial struct PendingSceneLoad : IComponent
{
    public required Handle<Scene> Scene;
}

public class ScenePlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
    ];

    public required int TypesVersion { get; set; } = 1;
    public ISceneFileTypeMigration[] FileTypeMigrations { get; set; } = [];

    public void Apply(World world)
    {
        var sceneSerializer = new SceneSerializer(1, TypesVersion)
        {
            FileTypeMigrations = FileTypeMigrations,
        };
        world.Resources.Get<AssetServer>().AddLoader(new SceneAssetLoader()
        {
            SceneSerializer = sceneSerializer,
        });
        world.Resources.Add(sceneSerializer);

        world.Schedule.AddSystemSet<SceneSystems>();
    }
}

public class SceneSerializer
{
    public int FormatVersion { get; init; }
    public int TypesVersion { get; init; }
    public required ISceneFileTypeMigration[] FileTypeMigrations { get; init; }

    public SceneSerializer(int formatVersion, int typesVersion)
    {
        this.FormatVersion = formatVersion;
        this.TypesVersion = typesVersion;
    }

    public SceneReader GetReader(SceneReader.Options options)
    {
        return new SceneReader(options with
        {
            FormatVersion = FormatVersion,
            TypesVersion = TypesVersion,
            FileTypeMigrations = FileTypeMigrations,
        });
    }

    public SceneWriter GetWriter(SceneWriter.Options options)
    {
        return new SceneWriter(options with
        {
            FormatVersion = FormatVersion,
            TypesVersion = TypesVersion,
        });
    }
}

[SystemSet]
public partial class SceneSystems
{
    [System(nameof(LoadScene))] static readonly SystemBuilderDescriptor LoadSceneDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    static void LoadScene(Commands commands, Assets<Scene> sceneAssets, Query<PendingSceneLoad> qPendingSceneLoad)
    {
        foreach (var pendingSceneLoad in qPendingSceneLoad)
        {
            var assetInfo = sceneAssets.GetInfo(pendingSceneLoad.Component0.Scene);
            if (assetInfo is null || assetInfo.Status != AssetStatus.Loaded || assetInfo.Asset is null)
            {
                continue;
            }

            commands.RemoveComponent<PendingSceneLoad>(pendingSceneLoad.Entity);
            SceneHelper.SpawnScene(commands, sceneAssets, pendingSceneLoad.Entity, pendingSceneLoad.Component0.Scene);
        }
    }
}