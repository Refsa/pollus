namespace Pollus.Engine.Rendering;

using System.Runtime.InteropServices;
using Core.Assets;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public class FontMeshRenderData
{
    public required Handle<GPUBuffer> VertexBuffer { get; init; }
    public uint VertexCount { get; set; }
    public uint VertexOffset { get; set; }

    public required Handle<GPUBuffer> IndexBuffer { get; init; }
    public IndexFormat IndexFormat { get; init; }
    public int IndexCount { get; set; }
    public uint IndexOffset { get; set; }
}

public class FontMeshRenderDataLoader : IRenderDataLoader
{
    public TypeID TargetType => TypeLookup.ID<TextMeshAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var textMeshAsset = assetServer.GetAssets<TextMeshAsset>().Get(handle)
                            ?? throw new InvalidOperationException("Text mesh asset not found");

        if (renderAssets.TryGet<FontMeshRenderData>(handle, out var fontMeshRenderData))
        {
            var vertexBuffer = renderAssets.Get(fontMeshRenderData.VertexBuffer);
            vertexBuffer.Resize<TextBuilder.TextVertex>((uint)textMeshAsset.Vertices.Count);
            vertexBuffer.Write(textMeshAsset.Vertices.AsSpan());

            var indexBuffer = renderAssets.Get(fontMeshRenderData.IndexBuffer);
            indexBuffer.Resize((uint)textMeshAsset.Indices.Count * sizeof(uint));
            var indexData = MemoryMarshal.Cast<uint, byte>(textMeshAsset.Indices.AsSpan());
            indexBuffer.Write(indexData, 0);

            fontMeshRenderData.VertexCount = (uint)textMeshAsset.Vertices.Count;
            fontMeshRenderData.IndexCount = textMeshAsset.Indices.Count;
        }
        else
        {
            var vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
                textMeshAsset.Name, Alignment.AlignedSize<TextBuilder.TextVertex>((uint)textMeshAsset.Vertices.Count)
            ));
            vertexBuffer.Write(textMeshAsset.Vertices.AsSpan());

            var indexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Index(textMeshAsset.Name, (ulong)textMeshAsset.Indices.Count * sizeof(uint)));
            var indexData = MemoryMarshal.Cast<uint, byte>(textMeshAsset.Indices.AsSpan());
            indexBuffer.Write(indexData, 0);

            renderAssets.Add(handle, new FontMeshRenderData
            {
                VertexBuffer = renderAssets.Add(vertexBuffer),
                VertexCount = (uint)textMeshAsset.Vertices.Count,
                VertexOffset = 0,

                IndexBuffer = renderAssets.Add(indexBuffer),
                IndexFormat = IndexFormat.Uint32,
                IndexCount = textMeshAsset.Indices.Count,
                IndexOffset = 0,
            });
        }
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var fontMesh = renderAssets.Get<FontMeshRenderData>(handle);
        renderAssets.Unload(fontMesh.VertexBuffer);
        renderAssets.Unload(fontMesh.IndexBuffer);
    }
}

[Asset]
public partial class FontMaterial : IMaterial
{
    public static string Name => "font";

    public static readonly VertexFormat[] InstanceFormats =
    [
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Color
    ];

    public static VertexBufferLayout[] VertexLayouts =>
    [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x2,
            VertexFormat.Float32x2,
            VertexFormat.Float32x4,
        ]),
        VertexBufferLayout.Instance(3, InstanceFormats),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "shape-render-pipeline",
        VertexState = new()
        {
            EntryPoint = "vs_main",
            Layouts = VertexLayouts,
        },
        FragmentState = new()
        {
            EntryPoint = "fs_main",
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default with
        {
            Topology = PrimitiveTopology.TriangleList,
            CullMode = CullMode.None,
            FrontFace = FrontFace.Ccw,
        },
    };

    public IBinding[][] Bindings =>
    [
        [new UniformBinding<SceneUniform>(), Texture, Sampler]
    ];

    public static BlendState? Blend => BlendState.Default with
    {
        Color = new BlendComponent
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
        Alpha = new BlendComponent
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
    };

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}

public record struct FontBatchKey
{
    readonly int hashCode;
    public readonly Handle<TextMeshAsset> TextMesh;
    public readonly Handle<FontMaterial> Material;

    public FontBatchKey(Handle<TextMeshAsset> textMesh, Handle<FontMaterial> material)
    {
        TextMesh = textMesh;
        Material = material;
        hashCode = HashCode.Combine(textMesh, material);
    }

    public override int GetHashCode() => hashCode;
}

public partial class FontBatch : RenderBatch<FontBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public Vec4f Model_0;
        public Vec4f Model_1;
        public Vec4f Model_2;
        public Vec4f Color;
    }

    public Handle<FontMaterial> Material { get; }
    public Handle<TextMeshAsset> TextMesh { get; }
    public override Handle[] RequiredResources => [Material, TextMesh];

    public FontBatch(in FontBatchKey key) : base(key.GetHashCode())
    {
        Material = key.Material;
        TextMesh = key.TextMesh;
    }

    public int Write(Mat4f model, Color color)
    {
        var tModel = model.Transpose();
        return Write(new InstanceData()
        {
            Model_0 = tModel.Col0,
            Model_1 = tModel.Col1,
            Model_2 = tModel.Col2,
            Color = color,
        });
    }
}

public class FontBatches : RenderBatches<FontBatch, FontBatchKey>
{
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var textMesh = renderAssets.Get<FontMeshRenderData>(batch.TextMesh);

        return Draw.Create(material.Pipeline)
            .SetVertexInfo(textMesh.VertexCount, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, textMesh.VertexBuffer)
            .SetVertexBuffer(1, batch.InstanceBufferHandle)
            .SetIndexBuffer(textMesh.IndexBuffer, textMesh.IndexFormat, (uint)textMesh.IndexCount, 0)
            .SetBindGroups(material.BindGroups);
    }

    protected override FontBatch CreateBatch(in FontBatchKey key)
    {
        return new FontBatch(key);
    }
}

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<Transform2D, TextDraw, TextMesh>>
{
    readonly struct Job : IForEach<Transform2D, TextDraw, TextMesh>
    {
        public required FontBatches Batches { get; init; }
        public required DrawQueue DrawQueue { get; init; }
        public required RendererKey RendererKey { get; init; }

        public void Execute(ref Transform2D transform, ref TextDraw textDraw, ref TextMesh textMesh)
        {
            if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textMesh.Material == Handle<FontMaterial>.Null) return;

            var batch = Batches.GetOrCreate(new FontBatchKey(textMesh.Mesh, textMesh.Material));
            var instanceIndex = batch.Write(transform.ToMat4f(), textDraw.Color);

            var sortKey = RenderingUtils.CreateSortKey2D(transform.Position.Y, batch.Key);
            DrawQueue.Add(sortKey, RendererKey.Key, batch.BatchID, instanceIndex);
        }
    }

    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<Transform2D, TextDraw, TextMesh> query, DrawQueue drawQueue)
    {
        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
            DrawQueue = drawQueue,
            RendererKey = RendererKey.From<FontBatches>(),
        });
    }
}