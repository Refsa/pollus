namespace Pollus.UI;

using Pollus.ECS;

public enum FocusSource
{
    Mouse,
    Keyboard,
}

public class UIFocusState
{
    public Entity FocusedEntity = Entity.Null;
    public FocusSource FocusSource;
    public List<Entity> FocusOrder = [];
}
