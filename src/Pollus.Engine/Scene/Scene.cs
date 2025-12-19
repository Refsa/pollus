namespace Pollus.Engine;

public class Scene
{
    public struct SceneEntity
    {
        public int EntityID { get; set; }
        public string? Name { get; set; }
        public List<EntityComponent>? Components { get; set; }
        public List<SceneEntity>? Children { get; set; }
    }

    public struct EntityComponent
    {
        public required int ComponentID { get; set; }
        public required byte[] Data { get; set; }
    }

    public required Dictionary<string, Type> Types { get; init; }
    public required List<SceneEntity> Entities { get; init; }
}