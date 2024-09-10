namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class DrawList
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

public class DrawGroup<TGroup>
    where TGroup : struct, Enum, IConvertible
{
    DrawList drawLists = new();

    public TGroup Group { get; init; }
    public DrawList DrawLists => drawLists;

    public void Execute(GPURenderPassEncoder encoder, IRenderAssets renderAssets)
    {
        Span<Handle<GPUBindGroup>> bindGroupHandles = stackalloc Handle<GPUBindGroup>[Draw.MAX_BIND_GROUPS] { Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null };
        Span<Handle<GPUBuffer>> vertexBufferHandles = stackalloc Handle<GPUBuffer>[Draw.MAX_VERTEX_BUFFERS] { Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null };
        Handle<GPUBuffer> indexBufferHandle = Handle<GPUBuffer>.Null;
        Handle<GPURenderPipeline> pipelineHandle = Handle<GPURenderPipeline>.Null;

        // TODO: Sort commands by resource usage
        foreach (var command in drawLists.Commands)
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

public class DrawGroups<TGroup>
    where TGroup : struct, Enum, IConvertible
{
    Dictionary<TGroup, DrawGroup<TGroup>> drawGroups = new();

    public IReadOnlyDictionary<TGroup, DrawGroup<TGroup>> Groups => drawGroups;

    public void Add(TGroup group)
    {
        var drawGroup = new DrawGroup<TGroup>()
        {
            Group = group
        };
        drawGroups.Add(group, drawGroup);
    }
    public DrawGroup<TGroup> Get(TGroup group) => drawGroups[group];
    public DrawList GetDrawList(TGroup group) => drawGroups[group].DrawLists;

    public void Cleanup()
    {
        foreach (var group in drawGroups.Values) group.DrawLists.Clear();
    }
}