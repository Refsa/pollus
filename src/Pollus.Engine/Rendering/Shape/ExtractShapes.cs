namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class ExtractShapesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, ShapeBatches, Query<Transform2, ShapeDraw>>
{
    struct Job : IForEach<Transform2, ShapeDraw>
    {
        public required ShapeBatches Batches { get; init; }

        public void Execute(ref Transform2 transform, ref ShapeDraw shape)
        {
            var batch = Batches.GetOrCreate(new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle));

            batch.Write(transform.ToMat4f(), shape.Color);
        }
    }

    public ExtractShapesSystem()
        : base(new ECS.Core.SystemDescriptor(nameof(ExtractShapesSystem)))
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
        });
    }
}

public class WriteShapeBatchesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, ShapeBatches>
{
    public WriteShapeBatchesSystem()
        : base(new ECS.Core.SystemDescriptor(nameof(WriteShapeBatchesSystem)).After(nameof(ExtractShapesSystem)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, ShapeBatches batches)
    {
        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            GPUBuffer? instanceBuffer;
            if (batch.InstanceBufferHandle == Handle<GPUBuffer>.Null)
            {
                instanceBuffer = batch.CreateBuffer(gpuContext);
                batch.InstanceBufferHandle = renderAssets.Add(instanceBuffer);
            }
            else
            {
                instanceBuffer = renderAssets.Get(batch.InstanceBufferHandle);
                batch.EnsureCapacity(instanceBuffer);
            }

            instanceBuffer.Write(batch.GetData());
        }
    }
}

public class DrawShapeBatchesSystem : ECS.Core.Sys<DrawGroups2D, RenderAssets, ShapeBatches>
{
    public DrawShapeBatchesSystem()
        : base(new ECS.Core.SystemDescriptor(nameof(DrawShapeBatchesSystem)).After(nameof(WriteShapeBatchesSystem)))
    { }

    protected override void OnTick(
        DrawGroups2D renderSteps, RenderAssets renderAssets, ShapeBatches batches)
    {
        var commands = renderSteps.GetDrawList(RenderStep2D.Main);

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

            var draw = new Draw()
            {
                Pipeline = material.Pipeline,
                VertexCount = shape.VertexCount,
                InstanceCount = (uint)batch.Count,
            };
            material.BindGroups.CopyTo(draw.BindGroups);
            draw.VertexBuffers[0] = shape.VertexBuffer;
            draw.VertexBuffers[1] = batch.InstanceBufferHandle;

            commands.Add(draw);
        }
    }
}