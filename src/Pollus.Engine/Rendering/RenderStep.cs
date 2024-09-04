namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public enum RenderStep2D
{
    Main,
    PostProcess,
    UI,
}

public class RenderSteps
{
    List<RenderStep2D> order = [RenderStep2D.Main, RenderStep2D.PostProcess, RenderStep2D.UI];
    Dictionary<RenderStep2D, List<IRenderStepDraw>> stages = new();

    public IReadOnlyDictionary<RenderStep2D, List<IRenderStepDraw>> Stages => stages;
    public IReadOnlyList<RenderStep2D> Order => order;

    public void Add(IRenderStepDraw stage)
    {
        if (!stages.TryGetValue(stage.Stage, out var list))
        {
            list = [];
            stages.Add(stage.Stage, list);
        }
        list.Add(stage);
    }
}

public interface IRenderStepDraw
{
    public RenderStep2D Stage { get; }
    void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets);

    public static void Draw(GPURenderPassEncoder encoder, RenderAssets renderAssets, in Draw draw)
    {
        var pipeline = renderAssets.Get<GPURenderPipeline>(draw.Pipeline);
        encoder.SetPipeline(pipeline);

        uint idx = 0;
        foreach (var bindGroup in draw.BindGroups)
        {
            if (bindGroup == Handle<GPUBindGroup>.Null) break;
            encoder.SetBindGroup(renderAssets.Get<GPUBindGroup>(bindGroup), idx++);
        }

        idx = 0;
        foreach (var vertexBuffer in draw.VertexBuffers)
        {
            if (vertexBuffer == Handle<GPUBuffer>.Null) break;
            encoder.SetVertexBuffer(idx++, renderAssets.Get<GPUBuffer>(vertexBuffer));
        }

        if (draw.IndexBuffer != Handle<GPUBuffer>.Null)
        {
            encoder.SetIndexBuffer(renderAssets.Get<GPUBuffer>(draw.IndexBuffer), IndexFormat.Uint16);
            encoder.DrawIndexed(draw.IndexCount, draw.InstanceCount, draw.IndexOffset, (int)draw.VertexOffset, draw.InstanceOffset);
        }
        else
        {
            encoder.Draw(draw.VertexCount, draw.InstanceCount, draw.VertexOffset, draw.InstanceOffset);
        }
    }
}