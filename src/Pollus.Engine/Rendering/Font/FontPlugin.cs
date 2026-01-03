namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.Graphics.WGPU;
using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public partial struct TextDraw : IComponent
{
    public static readonly EntityBuilder<TextDraw, TextMesh, Transform2D> Bundle = Entity.With(
        TextDraw.Default,
        TextMesh.Default,
        Transform2D.Default
    );

    public static readonly TextDraw Default = new TextDraw()
    {
        Font = Handle<FontAsset>.Null,
        Color = Color.WHITE,
        Size = 12f,
        Text = NativeUtf8.Null,
    };

    public required Handle<FontAsset> Font;
    public required Color Color;
    public required float Size;
    public bool IsDirty = false;

    NativeUtf8 text;

    public required NativeUtf8 Text
    {
        get => text;
        set
        {
            text.Dispose();
            text = value;
            IsDirty = true;
        }
    }

    public TextDraw()
    {
        text = NativeUtf8.Null;
    }
}

public partial struct TextMesh : IComponent
{
    public static readonly TextMesh Default = new TextMesh()
    {
        Mesh = Handle<TextMeshAsset>.Null,
        Material = Handle<FontMaterial>.Null,
    };

    public required Handle<TextMeshAsset> Mesh;
    public required Handle<FontMaterial> Material;
}

[Asset]
public partial class TextMeshAsset
{
    public required string Name { get; init; }
    public required ArrayList<TextBuilder.TextVertex> Vertices { get; init; }
    public required ArrayList<uint> Indices { get; init; }
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
        world.Resources.Add(new FontBatches());

        var registry = world.Resources.Get<RenderQueueRegistry>();
        var batches = world.Resources.Get<FontBatches>();
        registry.Register(RendererKey.From<FontBatches>().Key, batches);

        world.AddPlugin(MaterialPlugin<FontMaterial>.Default);

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractTextDrawSystem(),
        ]);

        world.Schedule.AddSystemSet<FontSystemSet>();
    }
}

[SystemSet]
public partial class FontSystemSet
{
    [System(nameof(PrepareFont))] static readonly SystemBuilderDescriptor PrepareFontDescriptor = new()
    {
        Stage = CoreStage.First,
        RunCriteria = EventRunCriteria<AssetEvent<FontAsset>>.Create,
    };

    [System(nameof(BuildTextMesh))] static readonly SystemBuilderDescriptor BuildTextMeshDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
    };

    [System(nameof(FontMaterialChanged))] static readonly SystemBuilderDescriptor FontMaterialChangedDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem, MaterialPlugin<FontMaterial>.PrepareSystem],
        RunCriteria = EventRunCriteria<AssetEvent<FontMaterial>>.Create,
    };

    [System(nameof(Cleanup))] static readonly SystemBuilderDescriptor CleanupDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    [System(nameof(PrepareTextMesh))] static readonly SystemBuilderDescriptor PrepareTextMeshDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
        RunCriteria = EventRunCriteria<AssetEvent<TextMeshAsset>>.Create,
    };

    static void PrepareFont(AssetServer assetServer, EventReader<AssetEvent<FontAsset>> fontEvents, Assets<FontAsset> fonts, Assets<FontMaterial> materials, Assets<SamplerAsset> samplers, Assets<Texture2D> textures)
    {
        foreach (scoped ref readonly var fontEvent in fontEvents.Read())
        {
            if (fontEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
            var font = fonts.Get(fontEvent.Handle);
            if (font is null) continue;

            font.Material = materials.Add(new FontMaterial()
            {
                ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/font.wgsl"),
                Sampler = samplers.Add(SamplerDescriptor.Default),
                Texture = font.Atlas,
            });
        }
    }

    static void FontMaterialChanged(Assets<FontAsset> fonts, EventReader<AssetEvent<FontMaterial>> fontMaterialEvents, Query<TextDraw, TextMesh> query)
    {
        foreach (scoped ref readonly var fontMaterialEvent in fontMaterialEvents.Read())
        {
            if (fontMaterialEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
            query.ForEach((fonts, fontMaterialEvent.Handle), static (in userData, ref draw, ref mesh) =>
            {
                var font = userData.fonts.Get(draw.Font);
                if (font?.Material != userData.Handle) return;
                mesh.Material = font.Material;
            });
        }
    }

    static void BuildTextMesh(Commands commands, Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes, Query<TextDraw, TextMesh> query)
    {
        query.ForEach((fonts, meshes), static (in userData, ref draw, ref mesh) =>
        {
            if (!draw.IsDirty) return;

            var font = userData.fonts.Get(draw.Font);
            if (font == null) return;

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
            TextBuilder.BuildMesh(draw.Text, font, Vec2f.Zero, draw.Color, draw.Size, tma.Vertices, tma.Indices);

            draw.IsDirty = false;
            userData.meshes.Set(mesh.Mesh, tma);
        });
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

    static void Cleanup(Assets<TextMeshAsset> meshes, RemovedTracker<TextMesh> textMeshTracker, RemovedTracker<TextDraw> textDrawTracker)
    {
        foreach (var entity in textMeshTracker)
        {
            if (entity.Component.Mesh == Handle<TextMeshAsset>.Null) continue;
            meshes.Remove(entity.Component.Mesh);
        }

        foreach (var entity in textDrawTracker)
        {
            entity.Component.Text.Dispose();
        }
    }
}