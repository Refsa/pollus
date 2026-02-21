namespace Pollus.Graphics;

using System.Runtime.CompilerServices;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
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
        for (int i = 0; i < count; i++) commands[i].Clear();
        count = 0;
    }
}

public class CommandList
{
    RenderCommands[] commands = new RenderCommands[4];
    int count;

    public ReadOnlySpan<RenderCommands> Commands => commands.AsSpan(0, count);

    public void Add(in RenderCommands command)
    {
        if (count == commands.Length)
        {
            var newCommands = new RenderCommands[commands.Length * 2];
            commands.CopyTo(newCommands, 0);
            commands = newCommands;
        }

        commands[count++] = command;
    }

    public void Clear()
    {
        foreach (var command in Commands) command.Dispose();
        Unsafe.SkipInit<RenderCommands>(out var def);
        Array.Fill(commands, def, 0, count);
        count = 0;
    }
}

public class DrawGroup<TGroup>
    where TGroup : struct, Enum, IConvertible
{
    readonly DrawList drawLists = new();
    readonly CommandList commandLists = new();

    public TGroup Group { get; init; }
    public DrawList DrawLists => drawLists;
    public CommandList CommandLists => commandLists;

    public void Clear()
    {
        drawLists.Clear();
        commandLists.Clear();
    }

    public void Execute(in GPURenderPassEncoder encoder, IRenderAssets renderAssets, Vec2<uint>? viewport = null)
    {
        Span<Handle<GPUBindGroup>> bindGroupHandles = stackalloc Handle<GPUBindGroup>[Draw.MAX_BIND_GROUPS] { Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null, Handle<GPUBindGroup>.Null };
        Span<Handle<GPUBuffer>> vertexBufferHandles = stackalloc Handle<GPUBuffer>[Draw.MAX_VERTEX_BUFFERS] { Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null, Handle<GPUBuffer>.Null };
        Handle<GPUBuffer> indexBufferHandle = Handle<GPUBuffer>.Null;
        Handle<GPURenderPipeline> pipelineHandle = Handle<GPURenderPipeline>.Null;
        RectInt? scissorRect = null;

        // TODO: Sort commands by resource usage
        foreach (scoped ref readonly var command in drawLists.Commands)
        {
            if (command.Pipeline != pipelineHandle)
            {
                pipelineHandle = command.Pipeline;
                encoder.SetPipeline(renderAssets.Get(pipelineHandle));
            }

            uint idx = 0;
            foreach (scoped ref readonly var bindGroup in command.BindGroups)
            {
                if (bindGroup == Handle<GPUBindGroup>.Null) break;
                if (bindGroupHandles[(int)idx] != bindGroup)
                {
                    bindGroupHandles[(int)idx] = bindGroup;
                    encoder.SetBindGroup(idx, renderAssets.Get(bindGroup));
                }

                idx++;
            }

            idx = 0;
            foreach (scoped ref readonly var vertexBuffer in command.VertexBuffers)
            {
                if (vertexBuffer == Handle<GPUBuffer>.Null) break;
                if (vertexBufferHandles[(int)idx] != vertexBuffer)
                {
                    vertexBufferHandles[(int)idx] = vertexBuffer;
                    encoder.SetVertexBuffer(idx, renderAssets.Get(vertexBuffer));
                }

                idx++;
            }

            if (!command.ScissorRect.NullableEquals(scissorRect))
            {
                scissorRect = command.ScissorRect;
                if (scissorRect.HasValue)
                {
                    var rect = scissorRect.Value;
                    var x = (uint)Math.Max(0, rect.Min.X);
                    var y = (uint)Math.Max(0, rect.Min.Y);
                    var w = (uint)Math.Max(0, rect.Width);
                    var h = (uint)Math.Max(0, rect.Height);
                    if (viewport.HasValue)
                    {
                        w = Math.Min(w, viewport.Value.X - Math.Min(x, viewport.Value.X));
                        h = Math.Min(h, viewport.Value.Y - Math.Min(y, viewport.Value.Y));
                    }
                    encoder.SetScissorRect(x, y, w, h);
                }
                else if (viewport.HasValue)
                {
                    // Reset scissor to full viewport when clip rect is removed
                    encoder.SetScissorRect(0, 0, viewport.Value.X, viewport.Value.Y);
                }
            }

            if (command.IndexBuffer != Handle<GPUBuffer>.Null)
            {
                if (indexBufferHandle != command.IndexBuffer)
                {
                    indexBufferHandle = command.IndexBuffer;
                    encoder.SetIndexBuffer(renderAssets.Get(command.IndexBuffer), command.IndexFormat);
                }

                encoder.DrawIndexed(command.IndexCount, command.InstanceCount, command.IndexOffset, (int)command.VertexOffset, command.InstanceOffset);
            }
            else
            {
                encoder.Draw(command.VertexCount, command.InstanceCount, command.VertexOffset, command.InstanceOffset);
            }
        }

        foreach (scoped ref readonly var command in commandLists.Commands)
        {
            command.Apply(encoder, renderAssets);
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

    public bool Has(TGroup group) => drawGroups.ContainsKey(group);
    public bool TryGet(TGroup group, out DrawGroup<TGroup> drawGroup) => drawGroups.TryGetValue(group, out drawGroup!);
    public DrawGroup<TGroup> Get(TGroup group) => drawGroups[group];
    public DrawList GetDrawList(TGroup group) => drawGroups[group].DrawLists;
    public CommandList GetCommandList(TGroup group) => drawGroups[group].CommandLists;

    public void Cleanup()
    {
        foreach (var group in drawGroups.Values)
        {
            group.Clear();
        }
    }
}