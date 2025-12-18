namespace Pollus.Engine;

using Pollus.Engine.Assets;
using Pollus.ECS;
using Utils;

public partial struct Prefab : IComponent
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
    public static Commands SpawnScene(this Commands commands, in Handle<Scene> scene, in Entity root)
    {
        commands.AddCommand(new SpawnSceneCommand { Scene = scene, Root = root });
        return commands;
    }
}

public struct SpawnSceneCommand : ICommand
{
    public static int Priority => 100;

    public required Handle<Scene> Scene;
    public required Entity Root;

    public void Execute(World world)
    {
        var scene = world.Resources.Get<AssetServer>().GetAssets<Scene>().Get(Scene);

        foreach (var sceneEntity in scene.Entities)
        {
            var entity = SpawnEntity(world, sceneEntity);
            if (Root != Entity.NULL)
            {
                new AddChildCommand { Parent = Root, Child = entity }.Execute(world);
            }
        }
    }

    static Entity SpawnEntity(World world, in Scene.Entity entity)
    {
        Span<ComponentID> cids = stackalloc ComponentID[entity.Components.Length];
        for (int i = 0; i < entity.Components.Length; i++)
        {
            cids[i] = entity.Components[i].ComponentID;
        }

        var aid = ArchetypeID.Create(cids);
        var entityRef = world.Store.CreateEntity(aid, cids);

        ref var chunk = ref entityRef.Archetype.Chunks[entityRef.ChunkIndex];
        for (int i = 0; i < entity.Components.Length; i++)
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