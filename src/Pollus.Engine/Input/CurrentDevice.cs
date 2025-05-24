namespace Pollus.Engine.Input;

public class CurrentDevice<TDevice>
    where TDevice : IInputDevice
{
    public TDevice? Value { get; set; }
}
