namespace Pollus.Input;

using Pollus.ECS;
using Pollus.Mathematics;

public interface IInputDevice : IDisposable
{
    Guid Id { get; }
    nint ExternalId { get; }
    InputType Type { get; }

    bool IsActive { get; }

    void PreUpdate();
    void Update(Events events);
}

public enum ButtonState
{
    None = 0,
    JustPressed = 1,
    Pressed = 2,
    JustReleased = 3,
}

public interface IButtonInputDevice<TButton>
    where TButton : Enum
{
    bool JustPressed(TButton button);
    bool Pressed(TButton button);
    bool JustReleased(TButton button);
}

public interface IAxisInputDevice<TAxis>
    where TAxis : Enum
{
    float GetAxis(TAxis axis);
    Vec2f GetAxis2D(TAxis xAxis, TAxis yAxis) => new(GetAxis(xAxis), GetAxis(yAxis));
}
