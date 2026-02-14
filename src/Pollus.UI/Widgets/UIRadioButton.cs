namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIRadioButton() : IComponent, IDefault<UIRadioButton>
{
    public static UIRadioButton Default => new();

    public int GroupId;
    public bool IsSelected;
    public Color SelectedColor = new(0.2f, 0.6f, 1.0f, 1f);
    public Color UnselectedColor = new(0.8f, 0.8f, 0.8f, 1f);
}

public static class UIRadioButtonEvents
{
    public struct UIRadioButtonEvent
    {
        public Entity Entity;
        public int GroupId;
        public bool IsSelected;
    }
}
