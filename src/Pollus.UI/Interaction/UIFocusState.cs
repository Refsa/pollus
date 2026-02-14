namespace Pollus.UI;

using Pollus.ECS;

public class UIFocusState
{
    public Entity FocusedEntity = Entity.Null;
    public List<Entity> FocusOrder = [];
}
