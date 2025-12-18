namespace Pollus.Engine;

public class Scene
{
    public struct Entity
    {
        public int EntityID { get; set; }
        public string? Name { get; set; }
        public List<Component> Components { get; set; }
        public List<Entity> Children { get; set; }
    }

    public struct Component
    {
        public required int TypeID { get; set; }
        public required int ComponentID { get; set; }
        public required byte[] Data { get; set; }
    }

    public struct Type
    {
        public required int ID { get; set; }
        public required string Name { get; set; }
        public required string AssemblyQualifiedName { get; set; }
    }

    public struct ComponentInfo
    {
        public required int TypeID { get; init; }
        public required int SizeInBytes { get; init; }
        public required bool Read { get; init; }
        public required bool Write { get; init; }
    }

    public required List<Type> Types { get; init; }
    public required List<ComponentInfo> ComponentInfos { get; init; }
    public required List<Entity> Entities { get; init; }
}
