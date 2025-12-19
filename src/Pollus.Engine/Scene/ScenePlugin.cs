namespace Pollus.Engine;

using Pollus.Engine.Assets;
using Pollus.ECS;
using Utils;

public partial struct SceneRoot : IComponent
{
    public required Handle<Scene> Scene;
}

public class ScenePlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
    ];

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().AddLoader<SceneAssetLoader>();
    }
}

public static class SceneCommands
{
    public static EntityCommands SpawnScene(this Commands commands, in Handle<Scene> scene)
    {
        var root = commands.Spawn();
        commands.AddCommand(new SpawnSceneCommand { Scene = scene, Root = root.Entity });
        return root;
    }
}

public struct SpawnSceneCommand : ICommand
{
    public static int Priority => 99;

    public required Handle<Scene> Scene;
    public required Entity Root;

    public void Execute(World world)
    {
        var scene = world.Resources.Get<AssetServer>().GetAssets<Scene>().Get(Scene);
        world.Store.AddComponent(Root, new SceneRoot { Scene = Scene });

        foreach (var sceneEntity in scene.Entities)
        {
            var entity = SpawnEntity(world, sceneEntity);
            new AddChildCommand { Parent = Root, Child = entity }.Execute(world);
        }
    }

    static Entity SpawnEntity(World world, in Scene.Entity entity)
    {
        Span<ComponentID> cids = stackalloc ComponentID[entity.Components.Count];
        for (int i = 0; i < entity.Components.Count; i++)
        {
            cids[i] = entity.Components[i].ComponentID;
        }

        var aid = ArchetypeID.Create(cids);
        var entityRef = world.Store.CreateEntity(aid, cids);

        ref var chunk = ref entityRef.Archetype.Chunks[entityRef.ChunkIndex];
        for (int i = 0; i < entity.Components.Count; i++)
        {
            chunk.SetComponent(entityRef.RowIndex, entity.Components[i].ComponentID, entity.Components[i].Data);
        }

        foreach (var sceneEntityChild in entity.Children)
        {
            var childEntity = SpawnEntity(world, sceneEntityChild);
            new AddChildCommand { Parent = entityRef.Entity, Child = childEntity }.Execute(world);
        }

        return entityRef.Entity;
    }
}