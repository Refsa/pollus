namespace Pollus.Engine;

using Pollus.Engine.Assets;
using Pollus.ECS;
using Utils;

public partial struct SceneRoot : IComponent
{
}

public partial struct SceneRef : IComponent
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
    }
}

public class SceneSerializer
{
    public int FormatVersion { get; init; }
    public int TypesVersion { get; init; }
    public ISceneFileTypeMigration[] FileTypeMigrations { get; init; }

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