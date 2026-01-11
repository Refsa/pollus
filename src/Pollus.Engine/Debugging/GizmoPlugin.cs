namespace Pollus.Debugging;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Rendering;
using Pollus.Graphics.WGPU;

public class GizmoPlugin : IPlugin
{
    static GizmoPlugin()
    {
        ResourceFetch<Gizmos>.Register();
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<FontPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Add(new Gizmos());
        world.AddPlugins(true, [
            new MaterialPlugin<GizmoFilledMaterial>(),
            new MaterialPlugin<GizmoOutlinedMaterial>(),
        ]);

        world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create(new("Gizmos::Setup"),
            static (
                Gizmos gizmos,
                AssetServer assetServer,
                Assets<ShaderAsset> shaders,
                Assets<GizmoFilledMaterial> filledMaterials,
                Assets<GizmoOutlinedMaterial> outlinedMaterials
            ) =>
            {
                var fontHandle = assetServer.Load<FontAsset>("builtin/fonts/SmoochSans-Light.ttf");
                var font = assetServer.GetAssets<FontAsset>().Get(fontHandle);
                Guard.IsNotNull(font, "GizmoPlugin::Setup: Font not found");
                gizmos.SetFont(font);

                var shader = shaders.Add(new ShaderAsset()
                {
                    Name = "gizmo",
                    Source = GizmoShaders.GIZMO_SHADER,
                });

                filledMaterials.Add(new GizmoFilledMaterial()
                {
                    ShaderSource = shader,
                });

                outlinedMaterials.Add(new GizmoOutlinedMaterial()
                {
                    ShaderSource = shader,
                });
            }));

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new("Gizmos::Draw")
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem]
            },
            static (Gizmos gizmos, IWGPUContext gpuContext,
                RenderAssets renderAssets, DrawGroups2D drawGroups
            ) =>
            {
                gizmos.PrepareFrame(gpuContext, renderAssets);

                if (gizmos.HasContent is false) return;
                var commands = drawGroups.GetCommandList(RenderStep2D.Last);
                gizmos.Dispatch(commands);
            }));
    }
}