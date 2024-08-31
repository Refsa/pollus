namespace Pollus.Engine.Rendering;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public class ShapePlugin : IPlugin
{
    static ShapePlugin()
    {
        AssetsFetch<Shape>.Register();
        AssetsFetch<ShapeMaterial>.Register();
        ResourceFetch<ShapeBatches>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<ShapeMaterial>());
        world.Resources.Get<RenderAssets>().AddLoader(new ShapeRenderDataLoader());
        world.Resources.Get<RenderSteps>().Add(new ShapeBatchDraw());
        world.Resources.Add(new ShapeBatches());

        world.Schedule.AddSystems(CoreStage.PreRender, [new ExtractShapesSystem()]);
    }
}

public struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
        },
        Transform2.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
}

public class ShapeMaterial : IMaterial
{
    public static string Name => "shape";

    public static readonly VertexFormat[] InstanceFormats = [
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Color
    ];

    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x2,
            VertexFormat.Float32x2,
        ]),
        VertexBufferLayout.Instance(2, InstanceFormats),
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
            Topology = PrimitiveTopology.TriangleStrip,
            CullMode = CullMode.None,
            FrontFace = FrontFace.Ccw,
        },
    };

    public IBinding[][] Bindings => [
        [new UniformBinding<SceneUniform>()]
    ];

    public required Handle<ShaderAsset> ShaderSource { get; set; }
}

public class ShapeBatch : IDisposable
{
    public int Key => Material.GetHashCode();
    public required Handle Material { get; init; }
    public required Handle<Shape> Shape { get; init; }
    public required GPUBuffer InstanceBuffer;

    VertexData instanceData;

    public int Count;
    public bool IsEmpty => Count == 0;
    public bool IsFull => Count == instanceData.Capacity;
    public int Capacity => (int)instanceData.Capacity;

    public ShapeBatch()
    {
        instanceData = VertexData.From(16, ShapeMaterial.InstanceFormats);
    }

    public void Dispose()
    {
        InstanceBuffer.Dispose();
    }

    public void Reset()
    {
        Count = 0;
    }

    public void Write(Mat4f model, Vec4f color)
    {
        var tModel = model.Transpose();
        instanceData.Write(Count++, tModel.Col0, tModel.Col1, tModel.Col2, color);
    }

    public void WriteBuffer()
    {
        InstanceBuffer.Write(instanceData.Slice(0, Count), 0);
    }

    public void Resize(IWGPUContext gpuContext, int capacity)
    {
        InstanceBuffer.Dispose();
        InstanceBuffer = gpuContext.CreateBuffer(new()
        {
            Label = $"InstanceBuffer_{Key}",
            Size = (ulong)capacity * (ulong)ShapeMaterial.InstanceFormats.Stride(),
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });

        instanceData.Resize((uint)capacity);
    }
}

public class ShapeBatches : IDisposable
{
    List<ShapeBatch> batches = new();
    Dictionary<int, int> batchLookup = new();

    public ListEnumerable<ShapeBatch> Batches => new(batches);

    public void Dispose()
    {
        foreach (var batch in batches)
        {
            batch.Dispose();
        }
        batches.Clear();
    }

    public bool TryGetBatch(Handle shapeHandle, Handle materialHandle, out ShapeBatch batch)
    {
        var key = HashCode.Combine(shapeHandle, materialHandle);
        if (batchLookup.TryGetValue(key, out var batchIdx))
        {
            batch = batches[batchIdx];
            return true;
        }
        batch = null!;
        return false;
    }

    public ShapeBatch CreateBatch(IWGPUContext context, int capacity, Handle materialHandle, Handle<Shape> shapeHandle)
    {
        var key = HashCode.Combine(shapeHandle, materialHandle);
        if (batchLookup.TryGetValue(key, out var batchIdx)) return batches[batchIdx];

        var batch = new ShapeBatch()
        {
            Material = materialHandle,
            Shape = shapeHandle,
            InstanceBuffer = context.CreateBuffer(new()
            {
                Label = $"InstanceBuffer_{key}",
                Size = (ulong)capacity * (ulong)SpriteMaterial.InstanceFormats.Stride(),
                Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
            }),
        };

        batchLookup.Add(key, batches.Count);
        batches.Add(batch);
        return batch;
    }

    public void Reset()
    {
        foreach (var batch in batches)
        {
            batch.Reset();
        }
    }
}

public class ShapeBatchDraw : IRenderStepDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<ShapeBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;
            batch.WriteBuffer();

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

            encoder.SetPipeline(material.Pipeline);
            for (int i = 0; i < material.BindGroups.Length; i++)
            {
                encoder.SetBindGroup(material.BindGroups[i], (uint)i);
            }

            encoder.SetVertexBuffer(0, shape.VertexBuffer);
            encoder.SetVertexBuffer(1, batch.InstanceBuffer);
            encoder.Draw(shape.VertexCount, (uint)batch.Count, 0, 0);
        }
    }
}

public class ShapeRenderData : IRenderData
{
    public required uint VertexCount { get; init; }
    public required GPUBuffer VertexBuffer { get; init; }

    public void Dispose()
    {
        VertexBuffer.Dispose();
    }
}

public class ShapeRenderDataLoader : IRenderDataLoader
{
    public int TargetType => AssetLookup.ID<Shape>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var shapeData = assetServer.GetAssets<Shape>().Get(handle)
            ?? throw new InvalidOperationException("Shape data not found");

        var vertexData = shapeData.GetVertexData();

        var vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
            shapeData.Name,
            vertexData.SizeInBytes
        ));
        vertexData.WriteTo(vertexBuffer, 0);

        renderAssets.Add(handle, new ShapeRenderData
        {
            VertexCount = vertexData.Count,
            VertexBuffer = vertexBuffer,
        });
    }
}

public class ExtractShapesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, ShapeBatches, Query<Transform2, ShapeDraw>>
{
    struct Job : IForEach<Transform2, ShapeDraw>
    {
        public required ShapeBatches Batches { get; init; }
        public required IWGPUContext GpuContext { get; init; }

        public void Execute(ref Transform2 transform, ref ShapeDraw shape)
        {
            if (!Batches.TryGetBatch(shape.ShapeHandle, shape.MaterialHandle, out var batch))
            {
                batch = Batches.CreateBatch(GpuContext, 16, shape.MaterialHandle, shape.ShapeHandle);
            }

            if (batch.IsFull)
            {
                batch.Resize(GpuContext, batch.Capacity * 2);
            }

            batch.Write(transform.ToMat4f(), shape.Color);
        }
    }

    public ExtractShapesSystem()
        : base(new ECS.Core.SystemDescriptor("ExtractShapes"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, ShapeBatches batches,
        Query<Transform2, ShapeDraw> query)
    {
        foreach (var shape in assetServer.GetAssets<Shape>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, shape.Handle);
        }
        foreach (var material in assetServer.GetAssets<ShapeMaterial>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, material.Handle);
        }

        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}