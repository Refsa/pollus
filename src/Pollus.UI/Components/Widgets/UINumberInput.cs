namespace Pollus.UI;

using Pollus.ECS;

public enum NumberInputType
{
    Float = 0,
    Int = 1,
}

public partial record struct UINumberInput() : IComponent, IDefault<UINumberInput>
{
    public static UINumberInput Default => new();

    public float Value;
    public float Min = float.MinValue;
    public float Max = float.MaxValue;
    public float Step = 1f;
    public NumberInputType Type = NumberInputType.Float;
    public Entity TextInputEntity = Entity.Null;
}

public static class UINumberInputEvents
{
    public struct UINumberInputValueChanged
    {
        public Entity Entity;
        public float Value;
        public float PreviousValue;
    }
}
