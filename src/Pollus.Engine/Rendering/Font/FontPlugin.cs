using Pollus.Graphics.WGPU;

namespace Pollus.Engine.Rendering;

using Pollus.Debugging;
using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public struct TextDraw : IComponent
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

public struct TextMesh : IComponent
{
    public static readonly TextMesh Default = new TextMesh()
    {
        Mesh = Handle<TextMeshAsset>.Null,
    };

    public Handle<TextMeshAsset> Mesh;
}

public class TextMeshAsset
{
    public required string Name { get; init; }
    public required ArrayList<TextBuilder.TextVertex> Vertices { get; init; }
    public required ArrayList<uint> Indices { get; init; }

    public bool IsDirty = true;
}

public class FontPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Init<FontAsset>();
        world.Resources.Get<AssetServer>().AddLoader<FontAssetLoader>();

        world.AddPlugin(MaterialPlugin<FontMaterial>.Create());
        world.Resources.Get<RenderAssets>().AddLoader<FontMeshRenderDataLoader>();
        world.Resources.Add(new FontBatches());
        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractTextDrawSystem(),
            new WriteBatchesSystem<FontBatches, FontBatch>(),
            new DrawBatchesSystem<FontBatches, FontBatch>()
            {
                RenderStep = RenderStep2D.Main,
                DrawExec = static (renderAssets, batch) =>
                {
                    var material = renderAssets.Get<MaterialRenderData>(batch.Material);
                    var textMesh = renderAssets.Get<FontMeshRenderData>(batch.TextMesh);

                    var draw = Draw.Create(material.Pipeline)
                        .SetVertexInfo(textMesh.VertexCount, 0)
                        .SetInstanceInfo((uint)batch.Count, 0)
                        .SetVertexBuffer(0, textMesh.VertexBuffer)
                        .SetVertexBuffer(1, batch.InstanceBufferHandle)
                        .SetIndexBuffer(textMesh.IndexBuffer, textMesh.IndexFormat, (uint)textMesh.IndexCount, 0)
                        .SetBindGroups(material.BindGroups);

                    return draw;
                },
            },
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
    };

    [System(nameof(BuildTextMesh))] static readonly SystemBuilderDescriptor BuildTextMeshDescriptor = new()
    {
        Stage = CoreStage.PreRender,
    };

    [System(nameof(Cleanup))] static readonly SystemBuilderDescriptor CleanupDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    [System(nameof(PrepareTextMesh))] static readonly SystemBuilderDescriptor PrepareTextMeshDescriptor = new()
    {
        Stage = CoreStage.PreRender,
    };

    static void PrepareFont(AssetServer assetServer, Assets<FontAsset> fonts, Assets<FontMaterial> materials, Assets<SamplerAsset> samplers, Assets<Texture2D> textures)
    {
        foreach (var font in fonts.AssetInfos)
        {
            if (font.Asset == null || font.Asset.Material != Handle<FontMaterial>.Null) continue;

            font.Asset.Material = materials.Add(new FontMaterial()
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/font.wgsl"),
                Sampler = samplers.Add(SamplerDescriptor.Default),
                Texture = textures.Add(font.Asset.Atlas),
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

            tma.IsDirty = true;
            draw.IsDirty = false;
        });
    }

    static void PrepareTextMesh(IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets)
    {
        foreach (var textMeshAsset in assetServer.GetAssets<TextMeshAsset>().AssetInfos)
        {
            if (textMeshAsset.Asset is not { IsDirty: true } tma) continue;
            renderAssets.Prepare(gpuContext, assetServer, textMeshAsset.Handle, true);
            tma.IsDirty = false;
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