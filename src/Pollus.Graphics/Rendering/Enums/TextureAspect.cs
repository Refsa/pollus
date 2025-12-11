namespace Pollus.Graphics.Rendering;

public enum TextureAspect : int
{
  All = 0x0,
  StencilOnly = 0x1,
  DepthOnly = 0x2,
  Force32 = 0x7FFFFFFF,
}