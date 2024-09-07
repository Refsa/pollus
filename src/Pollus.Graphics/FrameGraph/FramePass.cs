namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public interface IFramePass
{
    string Name { get; }
}

public class FramePass<TData> : IFramePass
{
    public string Name { get; }
    public TData Data { get; }

    public FramePass(string name, TData data)
    {
        Name = name;
        Data = data;
    }
}
