namespace Pollus.UI;

using Pollus.ECS;

public readonly struct SliderResult
{
    public Entity Entity { get; init; }

    public static implicit operator Entity(SliderResult result) => result.Entity;
}

public readonly struct TextInputResult
{
    public Entity Entity { get; init; }
    public Entity TextEntity { get; init; }

    public static implicit operator Entity(TextInputResult result) => result.Entity;
}

public readonly struct NumberInputResult
{
    public Entity Entity { get; init; }
    public Entity TextInputEntity { get; init; }
    public Entity TextEntity { get; init; }

    public static implicit operator Entity(NumberInputResult result) => result.Entity;
}

public readonly struct RadioGroupResult
{
    public Entity Entity { get; init; }
    public Entity[] OptionEntities { get; init; }

    public static implicit operator Entity(RadioGroupResult result) => result.Entity;
}

public readonly struct DropdownResult
{
    public Entity Entity { get; init; }
    public Entity DisplayTextEntity { get; init; }
    public Entity PopupPanelEntity { get; init; }
    public Entity[] OptionEntities { get; init; }
    public Entity[] OptionTextEntities { get; init; }

    public static implicit operator Entity(DropdownResult result) => result.Entity;
}
