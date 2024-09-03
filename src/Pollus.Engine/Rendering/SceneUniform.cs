#pragma warning disable CS0169

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

#pragma warning restore CS0169