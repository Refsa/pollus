namespace Pollus.Graphics.Rendering;

public struct Extent3D
{
    public uint Width;
    public uint Height;
    public uint DepthOrArrayLayers;

    public Extent3D(uint width, uint height, uint depthOrArrayLayers)
    {
        Width = width;
        Height = height;
        DepthOrArrayLayers = depthOrArrayLayers;
    }

    public static implicit operator Silk.NET.WebGPU.Extent3D(Extent3D extent)
    {
        return new Silk.NET.WebGPU.Extent3D
        {
            Width = extent.Width,
            Height = extent.Height,
            DepthOrArrayLayers = extent.DepthOrArrayLayers
        };
    }
}