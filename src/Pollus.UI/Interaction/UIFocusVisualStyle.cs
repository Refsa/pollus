namespace Pollus.UI;

using Pollus.Utils;

public class UIFocusVisualStyle
{
    public Color Color = new(0.26f, 0.52f, 0.96f, 1f);
    public float Width = 2f;
    public float Offset = 1f;
    /// <summary>When true, outline only appears for keyboard focus (like CSS :focus-visible).</summary>
    public bool KeyboardOnly = true;
}
