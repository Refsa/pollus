namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;


public class CameraPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create("Camera::Update", static (IWindow window, Query<Projection> query) =>
        {
            query.ForEach(new CameraProjectionUpdateForEach { WindowSize = window.Size });
        }));
    }
}

struct CameraProjectionUpdateForEach : IForEach<Projection>
{
    public required Vec2<uint> WindowSize { get; init; }

    public void Execute(ref Projection projection)
    {
        projection.Update(WindowSize);
    }
}