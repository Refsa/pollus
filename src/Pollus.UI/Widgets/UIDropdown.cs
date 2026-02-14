namespace Pollus.UI;

using Pollus.ECS;

public partial record struct UIDropdown() : IComponent
{
    public int SelectedIndex = -1;
    public bool IsOpen;
    public Entity PopupRootEntity = Entity.Null;
    public Entity DisplayTextEntity = Entity.Null;
}

public partial record struct UIDropdownOptionTag() : IComponent
{
    public Entity DropdownEntity = Entity.Null;
    public int OptionIndex;
}

public static class UIDropdownEvents
{
    public struct UIDropdownSelectionChanged
    {
        public Entity Entity;
        public int SelectedIndex;
        public int PreviousIndex;
    }
}
