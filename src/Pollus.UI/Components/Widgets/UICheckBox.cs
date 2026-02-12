namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UICheckBox() : IComponent, IDefault<UICheckBox>
{
    public static UICheckBox Default => new();

    public bool IsChecked;
    public Color CheckedColor = new(0.2f, 0.6f, 1.0f, 1f);
    public Color UncheckedColor = new(0.8f, 0.8f, 0.8f, 1f);
    public Color CheckmarkColor = new(1f, 1f, 1f, 1f);
}

public static class UICheckBoxEvents
{
    public struct UICheckBoxEvent
    {
        public Entity Entity;
        public bool IsChecked;
    }
}
