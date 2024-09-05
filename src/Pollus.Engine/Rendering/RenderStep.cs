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
        foreach (var command in commands.Commands)
        {
            var pipeline = renderAssets.Get<GPURenderPipeline>(command.Pipeline);
            encoder.SetPipeline(pipeline);

            uint idx = 0;
            foreach (var bindGroup in command.BindGroups)
            {
                if (bindGroup == Handle<GPUBindGroup>.Null) break;
                encoder.SetBindGroup(renderAssets.Get<GPUBindGroup>(bindGroup), idx++);
            }

            idx = 0;
            foreach (var vertexBuffer in command.VertexBuffers)
            {
                if (vertexBuffer == Handle<GPUBuffer>.Null) break;
                encoder.SetVertexBuffer(idx++, renderAssets.Get<GPUBuffer>(vertexBuffer));
            }

            if (command.IndexBuffer != Handle<GPUBuffer>.Null)
            {
                encoder.SetIndexBuffer(renderAssets.Get<GPUBuffer>(command.IndexBuffer), IndexFormat.Uint16);
                encoder.DrawIndexed(command.IndexCount, command.InstanceCount, command.IndexOffset, (int)command.VertexOffset, command.InstanceOffset);
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