using System.Runtime.CompilerServices;

namespace Pollus.Engine;

public class Scene
{
    public class Entity
    {
        public required int EntityID { get; set; }
        public required int Index { get; set; }
        public int[] Components { get; set; } = [];
    }

    public class Component
    {
        public required int TypeID { get; set; }
        public required int Index { get; set; }
        public required int Length { get; set; }
    }

    public class Resource
    {
        public required int TypeID { get; set; }
        public required int Index { get; set; }
        public required int Length { get; set; }
    }

    public class Type
    {
        public required int ID { get; set; }
        public required string Name { get; set; }
    }

    public Type[] Types { get; private set; } = [];
    public Entity[] Entities { get; private set; } = [];
    public Component[] Components { get; private set; } = [];
    public Resource[] Resources { get; private set; } = [];
}

