namespace Pollus.UI;

using Pollus.ECS;

public static class UIInteractionEvents
{
    public struct UIClickEvent { public Entity Entity; }
    public struct UIHoverEnterEvent { public Entity Entity; }
    public struct UIHoverExitEvent { public Entity Entity; }
    public struct UIPressEvent { public Entity Entity; }
    public struct UIReleaseEvent { public Entity Entity; }
    public struct UIFocusEvent { public Entity Entity; }
    public struct UIBlurEvent { public Entity Entity; }
    public struct UIKeyDownEvent { public Entity Entity; public int Key; }
    public struct UIKeyUpEvent { public Entity Entity; public int Key; }
    public struct UITextInputEvent { public Entity Entity; public string Text; }
    public struct UIDragEvent { public Entity Entity; public float PositionX; public float PositionY; public float DeltaX; public float DeltaY; }
}
