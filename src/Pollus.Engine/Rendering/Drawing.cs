namespace Pollus.Engine.Rendering;

using Pollus.Graphics;

public enum RenderStep2D
{
    First = 1000,
    Main = 2000,
    PostProcess = 3000,
    UI = 4000,
    Last = 5000,
}

public class DrawGroups2D : DrawGroups<RenderStep2D>
{
    public DrawGroups2D()
    {
        Add(RenderStep2D.First);
        Add(RenderStep2D.Main);
        Add(RenderStep2D.PostProcess);
        Add(RenderStep2D.UI);
        Add(RenderStep2D.Last);
    }
}

struct MainPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
}

struct UIPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
}