using Pollus.ECS;

namespace Pollus.UI;

public static class UIInteractionEvents
{
    public struct UIClickEvent { public Entity Entity; }
    public struct UIHoverEnterEvent { public Entity Entity; }
    public struct UIHoverExitEvent { public Entity Entity; }
    public struct UIPressEvent { public Entity Entity; }
    public struct UIReleaseEvent { public Entity Entity; }
    public struct UIFocusEvent { public Entity Entity; }
    public struct UIBlurEvent { public Entity Entity; }
}
