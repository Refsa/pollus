namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Camera;
using Pollus.Engine.Mesh;

public class RenderingPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.AddPlugins([
            new MeshPlugin(),
            new ImagePlugin(),
            new CameraPlugin(),
        ]);
    }
}