namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public enum RenderStep2D
{
    First = 1000,
    Main = 2000,
    PostProcess = 3000,
    UI = 4000,
    Last = 5000,
}

public class DrawCommands
{
    Draw[] commands = new Draw[4];
    int count;

    public ReadOnlySpan<Draw> Commands => commands.AsSpan(0, count);

    public void Add(in Draw draw)
    {
        if (count == commands.Length)
        {
            var newCommands = new Draw[commands.Length * 2];
            commands.CopyTo(newCommands, 0);
            commands = newCommands;
        }
        commands[count++] = draw;
    }

    public void Clear()
    {
        foreach (var command in commands) command.Clear();
        count = 0;
    }
}

public class RenderStep
{
    DrawCommands commands = new();
    RenderStep2D stage;

    public RenderStep2D Stage => stage;
    public DrawCommands Commands => commands;

    public RenderStep(RenderStep2D stage)
    {
        this.stage = stage;
    }

    public void Execute(GPURenderPassEncoder encoder, RenderAssets renderAssets)
    {
        Span<Handle<GPUBindGroup>> bindGroupHandles = stackalloc Handle<GPUBindGroup>[Draw.MAX_BIND_GROUPS] { Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null };
        Span<Handle<GPUBuffer>> vertexBufferHandles = stackalloc Handle<GPUBuffer>[Draw.MAX_VERTEX_BUFFERS] { Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null };
        Handle<GPUBuffer> indexBufferHandle = Handle<GPUBuffer>.Null;
        Handle<GPURenderPipeline> pipelineHandle = Handle<GPURenderPipeline>.Null;

        // TODO: Sort commands by resource usage
        foreach (var command in commands.Commands)
        {
            if (command.Pipeline != pipelineHandle)
            {
                pipelineHandle = command.Pipeline;
                encoder.SetPipeline(renderAssets.Get<GPURenderPipeline>(pipelineHandle));
            }

            uint idx = 0;
            foreach (var bindGroup in command.BindGroups)
            {
                if (bindGroup == Handle<GPUBindGroup>.Null) break;
                if (bindGroupHandles[(int)idx] != bindGroup)
                {
                    bindGroupHandles[(int)idx] = bindGroup;
                    encoder.SetBindGroup(idx, renderAssets.Get<GPUBindGroup>(bindGroup));
                }
                idx++;
            }

            idx = 0;
            foreach (var vertexBuffer in command.VertexBuffers)
            {
                if (vertexBuffer == Handle<GPUBuffer>.Null) break;
                if (vertexBufferHandles[(int)idx] != vertexBuffer)
                {
                    vertexBufferHandles[(int)idx] = vertexBuffer;
                    encoder.SetVertexBuffer(idx, renderAssets.Get<GPUBuffer>(vertexBuffer));
                }
                idx++;
            }

            if (command.IndexBuffer != Handle<GPUBuffer>.Null)
            {
                if (indexBufferHandle != command.IndexBuffer)
                {
                    indexBufferHandle = command.IndexBuffer;
                    encoder.SetIndexBuffer(renderAssets.Get<GPUBuffer>(command.IndexBuffer), IndexFormat.Uint16);
                }
            }
            else
            {
                encoder.Draw(command.VertexCount, command.InstanceCount, command.VertexOffset, command.InstanceOffset);
            }
        }
    }
}

public class RenderSteps
{
    List<RenderStep2D> order = [RenderStep2D.Main, RenderStep2D.PostProcess, RenderStep2D.UI];
    Dictionary<RenderStep2D, RenderStep> stages = new()
    {
        [RenderStep2D.Main] = new(RenderStep2D.Main),
        [RenderStep2D.PostProcess] = new(RenderStep2D.PostProcess),
        [RenderStep2D.UI] = new(RenderStep2D.UI),
    };

    public IReadOnlyDictionary<RenderStep2D, RenderStep> Stages => stages;
    public IReadOnlyList<RenderStep2D> Order => order;

    public RenderStep Get(RenderStep2D stage) => stages[stage];
    public DrawCommands GetCommands(RenderStep2D stage) => stages[stage].Commands;

    public void Cleanup()
    {
        foreach (var stage in stages.Values) stage.Commands.Clear();
    }
}