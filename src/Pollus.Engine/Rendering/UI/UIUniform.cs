namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;

[ShaderType]
public partial struct UIUniform
{
    public Vec2f ViewportSize;
    public float Time;
    public float DeltaTime;
    public Vec2f MousePosition;
    public float Scale;
}
