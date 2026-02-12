namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIDropdown() : IComponent, IDefault<UIDropdown>
{
    public static UIDropdown Default => new();

    public int SelectedIndex = -1;
    public bool IsOpen;
    public Entity PopupRootEntity = Entity.Null;
    public Entity DisplayTextEntity = Entity.Null;
}

public class UIDropdownOptions
{
    public List<string> Labels { get; } = [];

    public void Add(string label) => Labels.Add(label);
    public string Get(int index) => index >= 0 && index < Labels.Count ? Labels[index] : "";
    public int Count => Labels.Count;
}

public partial record struct UIDropdownOptionTag() : IComponent, IDefault<UIDropdownOptionTag>
{
    public static UIDropdownOptionTag Default => new();

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
