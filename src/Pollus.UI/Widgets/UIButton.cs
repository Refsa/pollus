namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIButton() : IComponent
{
    public Color NormalColor = new(0.8f, 0.8f, 0.8f, 1f);
    public Color HoverColor = new(0.9f, 0.9f, 0.9f, 1f);
    public Color PressedColor = new(0.6f, 0.6f, 0.6f, 1f);
    public Color DisabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);
}
