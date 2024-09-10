namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;

[ShaderType]
public partial struct SceneUniform
{
    public Mat4f View;
    public Mat4f Projection;
    public float Time;
}