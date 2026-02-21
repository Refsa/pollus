namespace Pollus.Engine.Rendering;

using Core.Assets;
using Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Collections;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Mathematics;
using Pollus.Utils;

[Asset]
public partial class TextMeshAsset
{
    public required string Name { get; init; }
    public required ArrayList<TextBuilder.TextVertex> Vertices { get; init; }
    public required ArrayList<uint> Indices { get; init; }

    public Rect Bounds { get; set; }
}

public class FontPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Init<FontAsset>();
        world.Resources.Get<AssetServer>().AddLoader<FontAssetLoader>();
        world.Resources.Get<RenderAssets>().AddLoader<FontMeshRenderDataLoader>();

        {
            var batches = new FontBatches()
            {
                RendererKey = RendererKey.From<FontBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

        world.AddPlugin(MaterialPlugin<FontMaterial>.Default);
        world.AddPlugin(TextDrawPlugin<TextDraw>.Default);

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractTextDrawSystem(),
        ]);

        world.Schedule.AddSystemSet<FontSystemSet>();
    }
}

[SystemSet]
public partial class FontSystemSet
{
    [System(nameof(PrepareFont))]
    public static readonly SystemBuilderDescriptor PrepareFontDescriptor = new()
    {
        Stage = CoreStage.First,
        RunCriteria = EventRunCriteria<AssetEvent<FontAsset>>.Create,
    };

    [System(nameof(FontMaterialChanged))]
    public static readonly SystemBuilderDescriptor FontMaterialChangedDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem, MaterialPlugin<FontMaterial>.PrepareSystem],
        RunCriteria = EventRunCriteria<AssetEvent<FontMaterial>>.Create,
    };

    [System(nameof(Cleanup))]
    public static readonly SystemBuilderDescriptor CleanupDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    [System(nameof(PrepareTextMesh))]
    public static readonly SystemBuilderDescriptor PrepareTextMeshDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
        RunCriteria = EventRunCriteria<AssetEvent<TextMeshAsset>>.Create,
    };

    static void PrepareFont(AssetServer assetServer, EventReader<AssetEvent<FontAsset>> fontEvents,
        Assets<FontAsset> fonts, Assets<FontMaterial> materials, Assets<SamplerAsset> samplers,
        Query<TextFont> qFonts)
    {
        foreach (scoped ref readonly var fontEvent in fontEvents.Read())
        {
            if (fontEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
            var font = fonts.Get(fontEvent.Handle);
            if (font is null) continue;

            font.Material = materials.Add(new FontMaterial()
            {
                ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/font.wgsl"),
                Sampler = assetServer.Load<SamplerAsset>("internal://samplers/linear"),
                Texture = font.Atlas,
            });
        }
    }

    static void FontMaterialChanged(Assets<FontAsset> fonts, EventReader<AssetEvent<FontMaterial>> fontMaterialEvents, Query<TextFont> query)
    {
        foreach (scoped ref readonly var fontMaterialEvent in fontMaterialEvents.Read())
        {
            if (fontMaterialEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
            query.ForEach((fonts, fontMaterialEvent.Handle), static (in userData, ref font) =>
            {
                var fontAsset = userData.fonts.Get(font.Font);
                if (fontAsset?.Material != userData.Handle) return;
                font.Material = fontAsset.Material;
            });
        }
    }

    static void PrepareTextMesh(IWGPUContext gpuContext, RenderAssets renderAssets, AssetServer assetServer, Assets<TextMeshAsset> meshes, EventReader<AssetEvent<TextMeshAsset>> textMeshAssetEvents)
    {
        foreach (scoped ref readonly var textMeshAssetEvent in textMeshAssetEvents.Read())
        {
            if (textMeshAssetEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;

            var textMeshAsset = meshes.Get(textMeshAssetEvent.Handle);
            if (textMeshAsset is null) continue;

            renderAssets.Prepare(gpuContext, assetServer, textMeshAssetEvent.Handle, true);
        }
    }

    static void Cleanup(Assets<TextMeshAsset> meshes, RemovedTracker<TextMesh> textMeshTracker)
    {
        foreach (var entity in textMeshTracker)
        {
            if (entity.Component.Mesh == Handle<TextMeshAsset>.Null) continue;
            meshes.Remove(entity.Component.Mesh);
        }
    }
}

public class TextDrawPlugin<C> : IPlugin
    where C : unmanaged, IComponent, ITextDraw
{
    public static readonly string CleanupSystem = $"TextDrawPlugin::{typeof(C).Name}::Cleanup";
    public static readonly string BuildTextMeshSystem = $"TextDrawPlugin::{typeof(C).Name}::BuildTextMesh";

    public static TextDrawPlugin<C> Default => new();

    public void Apply(World world)
    {
        world.Schedule.AddSystem(CoreStage.Last, FnSystem.Create(CleanupSystem,
            static (RemovedTracker<C> textDrawTracker) =>
            {
                foreach (var entity in textDrawTracker)
                {
                    entity.Component.Text.Dispose();
                }
            }));

        world.Schedule.AddSystem(CoreStage.First, FnSystem.Create(new(BuildTextMeshSystem)
            {
                RunsAfter = [FontSystemSet.PrepareFontDescriptor.Label],
            },
            static (Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes, Query<C, TextMesh, TextFont> query) =>
            {
                query.ForEach((fonts, meshes), static (in userData, ref draw, ref mesh, ref font) =>
                {
                    if (!draw.IsDirty) return;

                    var fontAsset = userData.fonts.Get(font.Font);
                    if (fontAsset == null) return;

                    var tier = fontAsset.GetTierForSize(draw.Size);

                    TextMeshAsset tma;
                    if (mesh.Mesh == Handle<TextMeshAsset>.Null)
                    {
                        tma = new TextMeshAsset
                        {
                            Name = $"TextMesh-{Guid.NewGuid()}",
                            Vertices = new(),
                            Indices = new(),
                        };
                        mesh.Mesh = userData.meshes.Add(tma);
                    }
                    else
                    {
                        tma = userData.meshes.Get(mesh.Mesh) ?? throw new InvalidOperationException("Text mesh asset not found");
                    }

                    tma.Vertices.Clear();
                    tma.Indices.Clear();
                    var result = TextBuilder.BuildMesh(draw.Text, tier, Vec2f.Zero, draw.Color, draw.Size, tma.Vertices, tma.Indices);
                    tma.Bounds = result.Bounds;

                    draw.IsDirty = false;
                    userData.meshes.Set(mesh.Mesh, tma);
                });
            }));
    }
}
