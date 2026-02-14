namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UISlider() : IComponent
{
    public float Value;
    public float Min;
    public float Max = 1f;
    public float Step;
    public Color TrackColor = new(0.3f, 0.3f, 0.3f, 1f);
    public Color FillColor = new(0.2f, 0.6f, 1.0f, 1f);
    public Color ThumbColor = new(1f, 1f, 1f, 1f);
    public Entity TrackEntity = Entity.Null;
    public Entity FillEntity = Entity.Null;
    public Entity ThumbEntity = Entity.Null;
}

public static class UISliderEvents
{
    public struct UISliderValueChanged
    {
        public Entity Entity;
        public float Value;
        public float PreviousValue;
    }
}
