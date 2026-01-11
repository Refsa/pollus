namespace Pollus.Debugging;

using Graphics;
using Graphics.Rendering;
using Graphics.Windowing;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Rendering;
using Pollus.Graphics.WGPU;

public class GizmoPlugin : IPlugin
{
    public const RenderStep2D GizmosPass = RenderStep2D.Last - 500;
    public const string GizmoSetupSystem = "GizmoPlugin::Setup";
    public const string GizmoDrawSystem = "GizmoPlugin::Draw";
    public const string GizmoFrameGraphSetupSystem = "GizmoPlugin::FrameGraphSetup";

    struct GizmosPassData
    {
        public ResourceHandle<TextureResource> ColorAttachment;
    }

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
        world.Resources.Get<DrawGroups2D>().Add(GizmosPass);

        world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create(new(GizmoSetupSystem),
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

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(GizmoDrawSystem)
            {
                RunsAfter = [GizmoFrameGraphSetupSystem]
            },
            static (Gizmos gizmos, IWGPUContext gpuContext,
                RenderAssets renderAssets, DrawGroups2D drawGroups
            ) =>
            {
                gizmos.PrepareFrame(gpuContext, renderAssets);

                if (gizmos.HasContent is false) return;
                var commands = drawGroups.GetCommandList(GizmosPass);
                gizmos.Dispatch(commands);
            }));

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(GizmoFrameGraphSetupSystem)
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem]
            },
            static (FrameGraph2D renderGraph, RenderContext renderContext, IWindow window,
                RenderAssets renderAssets, DrawGroups2D drawGroups, Resources resources) =>
            {
                var param = new FrameGraph2DParam()
                {
                    RenderAssets = renderAssets,
                    DrawGroups = drawGroups,
                    Resources = resources,
                    BackbufferFormat = renderContext.SurfaceTextureView!.Value.Descriptor.Format,
                    BackbufferSize = window.Size,
                };
                renderGraph.FrameGraph.AddPass(GizmosPass, param,
                    static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref GizmosPassData data) =>
                    {
                        data.ColorAttachment = builder.Writes<TextureResource>(FrameGraph2D.Textures.ColorTarget);
                    },
                    static (context, in param, in data) =>
                    {
                        ref var commandEncoder = ref context.GetCurrentCommandEncoder();
                        using var _debugScope = commandEncoder.DebugGroupScope("Gizmo Pass");
                        commandEncoder.InsertDebugMarker("Gizmo Pass");

                        using var passEncoder = commandEncoder.BeginRenderPass(new()
                        {
                            Label = "Gizmo Pass",
                            ColorAttachments = stackalloc RenderPassColorAttachment[]
                            {
                                new()
                                {
                                    View = context.Resources.GetTexture(data.ColorAttachment).TextureView.Native,
                                    LoadOp = LoadOp.Load,
                                    StoreOp = StoreOp.Store,
                                }
                            }
                        });

                        var stage = param.DrawGroups.Groups[GizmosPass];
                        stage.Execute(passEncoder, param.RenderAssets);
                    });
            }));
    }
}