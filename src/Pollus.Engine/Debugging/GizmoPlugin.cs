namespace Pollus.Debugging;

using Pollus.ECS;
using Pollus.Engine.Rendering;
using Pollus.Graphics.WGPU;

public class GizmoPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new Gizmos());

        /* world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create(new("Gizmos::Prepare"),
        static (Gizmos gizmos, IWGPUContext gpuContext, RenderAssets renderAssets) =>
        {
            gizmos.Prepare(gpuContext, renderAssets);
        })); */

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new("Gizmos::Draw"),
        static (Gizmos gizmos, IWGPUContext gpuContext, RenderAssets renderAssets, DrawGroups2D drawGroups) =>
        {
            gizmos.Prepare(gpuContext, renderAssets);

            var commands = drawGroups.GetCommandList(RenderStep2D.UI);
            gizmos.Dispatch(commands);
        }));
    }
}