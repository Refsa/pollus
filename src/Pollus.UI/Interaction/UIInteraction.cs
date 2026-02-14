namespace Pollus.UI;

using Pollus.ECS;

[Flags]
public enum InteractionState : byte
{
    None     = 0,
    Hovered  = 1 << 0,
    Pressed  = 1 << 1,
    Focused  = 1 << 2,
    Disabled = 1 << 3,
}

public partial record struct UIInteraction() : IComponent
{
    public InteractionState State;
    public int TabIndex;
    public bool Focusable;

    public readonly bool IsHovered => (State & InteractionState.Hovered) != 0;
    public readonly bool IsPressed => (State & InteractionState.Pressed) != 0;
    public readonly bool IsFocused => (State & InteractionState.Focused) != 0;
    public readonly bool IsDisabled => (State & InteractionState.Disabled) != 0;
}
