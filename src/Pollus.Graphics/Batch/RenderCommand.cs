namespace Pollus.Graphics;

using System.Buffers;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public enum RenderCommandType : ushort
{
    Undefined = 0,
    SetViewport,
    SetScissorRect,
    SetBlendConstant,
    SetPipeline,
    SetVertexBuffer,
    SetIndexBuffer,
    SetBindGroup,
    Draw,
    DrawIndexed,
    DrawIndirect,
    DrawIndexedIndirect,
    MultiDrawIndirect,
}

public interface IRenderCommand
{
    static abstract int SizeInBytes { get; }

    void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets);
}

public struct RenderCommands : IDisposable
{
    byte[] commands;
    int cursor;
    int count;

    public int Count => count;

    public RenderCommands()
    {
        commands = ArrayPool<byte>.Shared.Rent(256);
    }

    public static RenderCommands Builder => new();

    public void Dispose()
    {
        commands.AsSpan(0, count).Clear();
        ArrayPool<byte>.Shared.Return(commands);
    }

    void Resize()
    {
        byte[] newCommands = ArrayPool<byte>.Shared.Rent(commands.Length * 2);
        commands.CopyTo(newCommands, 0);
        ArrayPool<byte>.Shared.Return(commands);
        commands = newCommands;
    }

    void EnsureSize<TCommand>() where TCommand : unmanaged, IRenderCommand
    {
        if (cursor + TCommand.SizeInBytes > commands.Length)
            Resize();
    }

    TCommand ReadCommand<TCommand>(ref int offset)
        where TCommand : unmanaged, IRenderCommand
    {
        TCommand command = Unsafe.ReadUnaligned<TCommand>(ref commands[offset]);
        offset += TCommand.SizeInBytes;
        return command;
    }

    public void WriteCommand<TCommand>(in TCommand command) where TCommand : unmanaged, IRenderCommand
    {
        EnsureSize<SetViewportCommand>();
        MemoryMarshal.Write(commands.AsSpan(cursor), in command);
        cursor += TCommand.SizeInBytes;
        count++;
    }

    public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
    {
        int offset = 0;
        while (offset < cursor)
        {
            RenderCommandType type = Unsafe.ReadUnaligned<RenderCommandType>(ref commands[offset]);
            switch (type)
            {
                case RenderCommandType.SetViewport:
                    SetViewportCommand viewportCommand = ReadCommand<SetViewportCommand>(ref offset);
                    viewportCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetScissorRect:
                    SetScissorRectCommand scissorRectCommand = ReadCommand<SetScissorRectCommand>(ref offset);
                    scissorRectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetBlendConstant:
                    SetBlendConstantCommand blendConstantCommand = ReadCommand<SetBlendConstantCommand>(ref offset);
                    blendConstantCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetPipeline:
                    SetPipelineCommand pipelineCommand = ReadCommand<SetPipelineCommand>(ref offset);
                    pipelineCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetVertexBuffer:
                    SetVertexBufferCommand vertexBufferCommand = ReadCommand<SetVertexBufferCommand>(ref offset);
                    vertexBufferCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetIndexBuffer:
                    SetIndexBufferCommand indexBufferCommand = ReadCommand<SetIndexBufferCommand>(ref offset);
                    indexBufferCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetBindGroup:
                    SetBindGroupCommand bindGroupCommand = ReadCommand<SetBindGroupCommand>(ref offset);
                    bindGroupCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.Draw:
                    DrawCommand drawCommand = ReadCommand<DrawCommand>(ref offset);
                    drawCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndexed:
                    DrawIndexedCommand drawIndexedCommand = ReadCommand<DrawIndexedCommand>(ref offset);
                    drawIndexedCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndirect:
                    DrawIndirectCommand drawIndirectCommand = ReadCommand<DrawIndirectCommand>(ref offset);
                    drawIndirectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndexedIndirect:
                    DrawIndexedIndirectCommand drawIndexedIndirectCommand = ReadCommand<DrawIndexedIndirectCommand>(ref offset);
                    drawIndexedIndirectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.MultiDrawIndirect:
                    MultiDrawIndirectCommand multiDrawIndirectCommand = ReadCommand<MultiDrawIndirectCommand>(ref offset);
                    multiDrawIndirectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                default:
                    throw new NotImplementedException($"Unknown command type: {type}");
            }
        }
    }

    public void ApplyAndDispose(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
    {
        Apply(renderPassEncoder, renderAssets);
        Dispose();
    }

    public RenderCommands SetViewport(Vec2f origin, Vec2f size, float minDepth, float maxDepth)
    {
        WriteCommand(new SetViewportCommand
        {
            Origin = origin,
            Size = size,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
        });
        return this;
    }

    public RenderCommands SetScissorRect(uint x, uint y, uint width, uint height)
    {
        WriteCommand(new SetScissorRectCommand
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
        });
        return this;
    }

    public RenderCommands SetBlendConstant(Vec4<double> blendConstant)
    {
        WriteCommand(new SetBlendConstantCommand
        {
            BlendConstant = blendConstant,
        });
        return this;
    }

    public RenderCommands SetPipeline(Handle<GPURenderPipeline> pipeline)
    {
        WriteCommand(new SetPipelineCommand
        {
            Pipeline = pipeline,
        });
        return this;
    }

    public RenderCommands SetVertexBuffer(uint slot, Handle<GPUBuffer> buffer, uint? offset = null, uint? size = null)
    {
        WriteCommand(new SetVertexBufferCommand
        {
            Slot = slot,
            Buffer = buffer,
            Offset = offset,
            Size = size,
        });
        return this;
    }

    public RenderCommands SetIndexBuffer(Handle<GPUBuffer> buffer, IndexFormat format, uint? offset = null, uint? size = null)
    {
        WriteCommand(new SetIndexBufferCommand
        {
            Buffer = buffer,
            Format = format,
            Offset = offset,
            Size = size,
        });
        return this;
    }

    public RenderCommands SetBindGroup(uint groupIndex, Handle<GPUBindGroup> bindGroup, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
    {
        WriteCommand(new SetBindGroupCommand
        {
            GroupIndex = groupIndex,
            BindGroup = bindGroup,
            DynamicOffsetCount = dynamicOffsetCount,
            DynamicOffsets = dynamicOffsets,
        });
        return this;
    }

    public RenderCommands SetBindGroups(uint groupIndex, Handle<GPUBindGroup>[] bindGroups, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
    {
        for (int i = 0; i < bindGroups.Length; i++)
        {
            SetBindGroup(groupIndex + (uint)i, bindGroups[i], dynamicOffsetCount, dynamicOffsets);
        }
        return this;
    }

    public RenderCommands Draw(uint vertexCount, uint instanceCount, uint vertexOffset = 0, uint instanceOffset = 0)
    {
        WriteCommand(new DrawCommand
        {
            VertexCount = vertexCount,
            InstanceCount = instanceCount,
            VertexOffset = vertexOffset,
            InstanceOffset = instanceOffset,
        });
        return this;
    }

    public RenderCommands DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex = 0, int baseVertex = 0, uint firstInstance = 0)
    {
        WriteCommand(new DrawIndexedCommand
        {
            IndexCount = indexCount,
            InstanceCount = instanceCount,
            FirstIndex = firstIndex,
            BaseVertex = baseVertex,
            FirstInstance = firstInstance,
        });
        return this;
    }

    public RenderCommands DrawIndirect(Handle<GPUBuffer> indirectBuffer, uint indirectOffset)
    {
        WriteCommand(new DrawIndirectCommand
        {
            IndirectBuffer = indirectBuffer,
            IndirectOffset = indirectOffset,
        });
        return this;
    }

    public RenderCommands DrawIndexedIndirect(Handle<GPUBuffer> indirectBuffer, uint indirectOffset)
    {
        WriteCommand(new DrawIndexedIndirectCommand
        {
            IndirectBuffer = indirectBuffer,
            IndirectOffset = indirectOffset,
        });
        return this;
    }

    public RenderCommands MultiDrawIndirect(Handle<GPUBuffer> indirectBuffer, uint count)
    {
        WriteCommand(new MultiDrawIndirectCommand
        {
            IndirectBuffer = indirectBuffer,
            Count = count,
        });
        return this;
    }

    public struct SetViewportCommand : IRenderCommand
    {
        static readonly int sizeInBytes = Unsafe.SizeOf<SetViewportCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetViewportCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetViewport;
        public required Vec2f Origin;
        public required Vec2f Size;
        public required float MinDepth;
        public required float MaxDepth;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.SetViewport(Origin, Size, MinDepth, MaxDepth);
        }
    }

    public struct SetScissorRectCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetScissorRectCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetScissorRectCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetScissorRect;
        public required uint X;
        public required uint Y;
        public required uint Width;
        public required uint Height;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.SetScissorRect(X, Y, Width, Height);
        }
    }

    public struct SetBlendConstantCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetBlendConstantCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetBlendConstantCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetBlendConstant;
        public required Vec4<double> BlendConstant;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.SetBlendConstant(BlendConstant);
        }
    }

    public struct SetPipelineCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetPipelineCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetPipelineCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetPipeline;
        public required Handle<GPURenderPipeline> Pipeline;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var pipeline = renderAssets.Get(Pipeline);
            renderPassEncoder.SetPipeline(pipeline);
        }
    }

    public struct SetVertexBufferCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetVertexBufferCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetVertexBufferCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetVertexBuffer;
        public required uint Slot;
        public required Handle<GPUBuffer> Buffer;
        public uint? Offset;
        public uint? Size;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(Buffer);
            renderPassEncoder.SetVertexBuffer(Slot, buffer, Offset, Size);
        }
    }

    public struct SetIndexBufferCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetIndexBufferCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetIndexBufferCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetIndexBuffer;
        public required Handle<GPUBuffer> Buffer;
        public required IndexFormat Format;
        public uint? Offset;
        public uint? Size;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(Buffer);
            renderPassEncoder.SetIndexBuffer(buffer, Format, Offset, Size);
        }
    }

    public struct SetBindGroupCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetBindGroupCommand>();
        public static int SizeInBytes => sizeInBytes;

        public SetBindGroupCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.SetBindGroup;
        public required uint GroupIndex;
        public required Handle<GPUBindGroup> BindGroup;
        public uint DynamicOffsetCount;
        public uint DynamicOffsets;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var bindGroup = renderAssets.Get(BindGroup);
            renderPassEncoder.SetBindGroup(GroupIndex, bindGroup, DynamicOffsetCount, DynamicOffsets);
        }
    }

    public struct DrawCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawCommand>();
        public static int SizeInBytes => sizeInBytes;

        public DrawCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.Draw;
        public required uint VertexCount;
        public required uint InstanceCount;
        public uint VertexOffset;
        public uint InstanceOffset;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.Draw(VertexCount, InstanceCount, VertexOffset, InstanceOffset);
        }
    }

    public struct DrawIndexedCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndexedCommand>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndexedCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.DrawIndexed;
        public required uint IndexCount;
        public required uint InstanceCount;
        public uint FirstIndex;
        public int BaseVertex;
        public uint FirstInstance;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.DrawIndexed(IndexCount, InstanceCount, FirstIndex, BaseVertex, FirstInstance);
        }
    }

    public struct DrawIndirectCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndirectCommand>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndirectCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.DrawIndirect;
        public required Handle<GPUBuffer> IndirectBuffer;
        public uint IndirectOffset;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(IndirectBuffer);
            renderPassEncoder.DrawIndirect(buffer, IndirectOffset);
        }
    }

    public struct MultiDrawIndirectCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<MultiDrawIndirectCommand>();
        public static int SizeInBytes => sizeInBytes;

        public MultiDrawIndirectCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.MultiDrawIndirect;
        public required Handle<GPUBuffer> IndirectBuffer;
        public uint Count;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(IndirectBuffer);
            renderPassEncoder.DrawIndirectMulti(buffer, Count);
        }
    }

    public struct DrawIndexedIndirectCommand : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndexedIndirectCommand>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndexedIndirectCommand() { }

        public readonly RenderCommandType Type = RenderCommandType.DrawIndexedIndirect;
        public required Handle<GPUBuffer> IndirectBuffer;
        public uint IndirectOffset;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(IndirectBuffer);
            renderPassEncoder.DrawIndexedIndirect(buffer, IndirectOffset);
        }
    }
}