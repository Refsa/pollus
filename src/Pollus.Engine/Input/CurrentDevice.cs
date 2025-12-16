namespace Pollus.Engine.Input;

using Pollus.ECS;

public class CurrentDevice<TDevice>
    where TDevice : IInputDevice
{
    static CurrentDevice()
    {
        ResourceFetch<CurrentDevice<TDevice>>.Register();
    }

    public TDevice? Value { get; set; }
}
