using Pollus.ECS;

namespace Pollus.UI;

public class UIFocusState
{
    public Entity FocusedEntity = Entity.Null;
    public List<Entity> FocusOrder = [];
}
