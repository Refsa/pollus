namespace Pollus.Engine.Rendering;

using Pollus.Collections;
using Pollus.Core.Assets;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public class UITextPlugin : IPlugin
{
    public const string PrepareUIFontSystem = "UITextPlugin::PrepareUIFont";
    public const string PropagateUIFontMaterialSystem = "UITextPlugin::PropagateUIFontMaterial";
    public const string RegisterMeasureFuncsSystem = "UITextPlugin::RegisterMeasureFuncs";
    public const string BuildUITextMeshSystem = "UITextPlugin::BuildUITextMesh";
    public const string CleanupSystem = "UITextPlugin::Cleanup";

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<UIRenderPlugin>(),
        PluginDependency.From<FontPlugin>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugin(new MaterialPlugin<UIFontMaterial>());

        {
            var batches = new UIFontBatches()
            {
                RendererKey = RendererKey.From<UIFontBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

        world.Resources.Add(new UITextResources());

        // PrepareUIFont: Create UIFontMaterial per font on FontAsset load
        world.Schedule.AddSystem(CoreStage.First, FnSystem.Create(new(PrepareUIFontSystem)
        {
            RunCriteria = EventRunCriteria<AssetEvent<FontAsset>>.Create,
        },
        static (AssetServer assetServer, UITextResources uiTextResources,
            EventReader<AssetEvent<FontAsset>> fontEvents,
            Assets<FontAsset> fonts, Assets<UIFontMaterial> materials, Assets<SamplerAsset> samplers) =>
        {
            foreach (scoped ref readonly var fontEvent in fontEvents.Read())
            {
                if (fontEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
                var font = fonts.Get(fontEvent.Handle);
                if (font is null) continue;

                var mat = materials.Add(new UIFontMaterial()
                {
                    ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/ui_font.wgsl"),
                    Sampler = assetServer.Load<SamplerAsset>("internal://samplers/linear"),
                    Texture = font.Atlas,
                });
                uiTextResources.SetMaterial(fontEvent.Handle, mat);
            }
        }));

        // PropagateUIFontMaterial: Set UITextFont.Material from UITextResources
        world.Schedule.AddSystem(CoreStage.PreRender, FnSystem.Create(new(PropagateUIFontMaterialSystem)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem, MaterialPlugin<UIFontMaterial>.PrepareSystem],
        },
        static (UITextResources uiTextResources, Query<UITextFont> query) =>
        {
            query.ForEach(uiTextResources, static (in UITextResources res, ref UITextFont textFont) =>
            {
                if (textFont.Font == Handle<FontAsset>.Null) return;
                var mat = res.GetMaterial(textFont.Font);
                if (mat != Handle<UIFontMaterial>.Null)
                {
                    textFont.Material = mat;
                }
            });
        }));

        // RegisterMeasureFuncs: Register measure delegates for UIText entities
        world.Schedule.AddSystem(CoreStage.First, FnSystem.Create(new(RegisterMeasureFuncsSystem)
        {
            RunsAfter = [PrepareUIFontSystem],
        },
        static (UITreeAdapter adapter, Assets<FontAsset> fonts, Query<UIText, ContentSize, UITextFont> query) =>
        {
            foreach (var row in query)
            {
                ref var uiText = ref row.Component0;
                ref var textFont = ref row.Component2;
                var entity = row.Entity;

                if (!uiText.IsDirty && adapter.GetNodeId(entity) >= 0)
                {
                    // Already registered and not dirty, skip
                    continue;
                }

                if (textFont.Font == Handle<FontAsset>.Null) continue;
                var fontAsset = fonts.Get(textFont.Font);
                if (fontAsset is null) continue;

                // Capture current text/size/font for the measure delegate
                var capturedText = uiText.Text;
                var capturedSize = uiText.Size;
                var capturedFont = fontAsset;

                adapter.SetMeasureFunc(entity, (knownDimensions, availableSpace) =>
                {
                    float maxWidth = knownDimensions.Width
                        ?? availableSpace.Width.AsDefinite()
                        ?? 0f;
                    var measured = TextBuilder.MeasureText(capturedText, capturedFont, capturedSize, maxWidth);
                    return new Size<float>(
                        knownDimensions.Width ?? measured.X,
                        knownDimensions.Height ?? measured.Y);
                });

                uiText.IsDirty = false;
            }
        }));

        // BuildUITextMesh: Build text mesh for dirty UIText entities
        // Runs in CoreStage.Last so ComputedNode is populated from layout (CoreStage.PostUpdate)
        world.Schedule.AddSystem(CoreStage.Last, FnSystem.Create(new(BuildUITextMeshSystem),
        static (Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes, Query<UIText, TextMesh, UITextFont, ComputedNode> query) =>
        {
            query.ForEach((fonts, meshes), static (in (Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes) data, ref UIText uiText, ref TextMesh mesh, ref UITextFont textFont, ref ComputedNode computed) =>
            {
                float maxWidth = computed.Size.X - computed.PaddingLeft - computed.PaddingRight
                                 - computed.BorderLeft - computed.BorderRight;
                if (maxWidth < 0f) maxWidth = 0f;

                bool widthChanged = uiText.LastBuildMaxWidth != maxWidth;
                if (!uiText.IsDirty && !widthChanged) return;

                var fontAsset = data.fonts.Get(textFont.Font);
                if (fontAsset == null) return;

                TextMeshAsset tma;
                if (mesh.Mesh == Handle<TextMeshAsset>.Null)
                {
                    tma = new TextMeshAsset
                    {
                        Name = $"UITextMesh-{Guid.NewGuid()}",
                        Vertices = new(),
                        Indices = new(),
                    };
                    mesh.Mesh = data.meshes.Add(tma);
                }
                else
                {
                    tma = data.meshes.Get(mesh.Mesh) ?? throw new InvalidOperationException("Text mesh asset not found");
                }

                tma.Vertices.Clear();
                tma.Indices.Clear();
                var result = TextBuilder.BuildMesh(uiText.Text, fontAsset, Vec2f.Zero, Vec4f.One, uiText.Size, tma.Vertices, tma.Indices, TextCoordinateMode.YDown, maxWidth);
                tma.Bounds = result.Bounds;

                uiText.LastBuildMaxWidth = maxWidth;
                data.meshes.Set(mesh.Mesh, tma);
            });
        }));

        // ExtractUIText
        world.Schedule.AddSystems(CoreStage.PreRender, ExtractUITextSystem.Create());

        // Cleanup: Dispose NativeUtf8 and remove measure funcs
        world.Schedule.AddSystem(CoreStage.Last, FnSystem.Create(new(CleanupSystem)
        {
            RunsAfter = [BuildUITextMeshSystem],
        },
            static (UITreeAdapter adapter, RemovedTracker<UIText> tracker) =>
            {
                foreach (var removed in tracker)
                {
                    removed.Component.Text.Dispose();
                    adapter.RemoveMeasureFunc(new Entity(removed.Entity));
                }
            }));
    }
}
