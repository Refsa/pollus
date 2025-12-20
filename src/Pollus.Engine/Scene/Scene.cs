namespace Pollus.Engine;

using System.Text.Json;
using System.Text.Json.Serialization;

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

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(SceneFileData))]
[JsonSerializable(typeof(SceneFileData.EntityData))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<SceneFileData.EntityData>))]
[JsonSerializable(typeof(Dictionary<string, SceneFileData.EntityData>))]
internal sealed partial class SceneFileDataJsonSerializerContext : JsonSerializerContext
{
    public static SceneFileDataJsonSerializerContext Indented = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        IndentSize = 2,
    });
}

public struct SceneFileData
{
    public struct EntityData
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, JsonElement>? Components { get; set; }
        public List<EntityData>? Children { get; set; }
    }

    public Dictionary<string, string>? Types { get; set; }
    public List<EntityData>? Entities { get; set; }
}