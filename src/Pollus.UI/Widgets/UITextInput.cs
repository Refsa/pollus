namespace Pollus.UI;

using Pollus.ECS;

public enum UIInputFilterType
{
    Any = 0,
    Integer = 1,
    Decimal = 2,
    Alphanumeric = 3,
}

public partial record struct UITextInput() : IComponent
{
    public int CursorPosition;
    public UIInputFilterType Filter = UIInputFilterType.Any;
    public float CaretBlinkTimer;
    public float CaretBlinkRate = 0.53f;
    public bool CaretVisible = true;
    public Entity TextEntity = Entity.Null;
    public float CaretXOffset;
    public float CaretHeight;
    public Entity CaretEntity = Entity.Null;
}

/// <summary>
/// Managed resource storing text content for UITextInput entities.
/// Text is stored separately from the component because strings are managed types
/// and ECS components must be unmanaged.
/// </summary>
public class UITextBuffers
{
    readonly Dictionary<Entity, string> buffers = [];

    public string Get(Entity entity) => buffers.TryGetValue(entity, out var text) ? text : "";

    public void Set(Entity entity, string text) => buffers[entity] = text;

    public void Remove(Entity entity) => buffers.Remove(entity);

    public bool Has(Entity entity) => buffers.ContainsKey(entity);
}

public static class UITextInputEvents
{
    public struct UITextInputValueChanged
    {
        public Entity Entity;
    }
}
