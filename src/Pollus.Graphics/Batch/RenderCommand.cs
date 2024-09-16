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

    public void WriteCommand<TCommand>(in TCommand command) where TCommand : unmanaged, IRenderCommand
    {
        EnsureSize<SetViewport>();
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
                    SetViewport viewportCommand = ReadCommand<SetViewport>(ref offset);
                    viewportCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetScissorRect:
                    SetScissorRect scissorRectCommand = ReadCommand<SetScissorRect>(ref offset);
                    scissorRectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetBlendConstant:
                    SetBlendConstant blendConstantCommand = ReadCommand<SetBlendConstant>(ref offset);
                    blendConstantCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetPipeline:
                    SetPipeline pipelineCommand = ReadCommand<SetPipeline>(ref offset);
                    pipelineCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetVertexBuffer:
                    SetVertexBuffer vertexBufferCommand = ReadCommand<SetVertexBuffer>(ref offset);
                    vertexBufferCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetIndexBuffer:
                    SetIndexBuffer indexBufferCommand = ReadCommand<SetIndexBuffer>(ref offset);
                    indexBufferCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.SetBindGroup:
                    SetBindGroup bindGroupCommand = ReadCommand<SetBindGroup>(ref offset);
                    bindGroupCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.Draw:
                    Draw drawCommand = ReadCommand<Draw>(ref offset);
                    drawCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndexed:
                    DrawIndexed drawIndexedCommand = ReadCommand<DrawIndexed>(ref offset);
                    drawIndexedCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndirect:
                    DrawIndirect drawIndirectCommand = ReadCommand<DrawIndirect>(ref offset);
                    drawIndirectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                case RenderCommandType.DrawIndexedIndirect:
                    DrawIndexedIndirect drawIndexedIndirectCommand = ReadCommand<DrawIndexedIndirect>(ref offset);
                    drawIndexedIndirectCommand.Apply(renderPassEncoder, renderAssets);
                    break;
                default:
                    throw new NotImplementedException($"Unknown command type: {type}");
            }
        }
    }

    TCommand ReadCommand<TCommand>(ref int offset)
        where TCommand : unmanaged, IRenderCommand
    {
        TCommand command = Unsafe.ReadUnaligned<TCommand>(ref commands[offset]);
        offset += TCommand.SizeInBytes;
        return command;
    }

    public struct SetViewport : IRenderCommand
    {
        static readonly int sizeInBytes = Unsafe.SizeOf<SetViewport>();
        public static int SizeInBytes => sizeInBytes;

        public SetViewport() { }

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

    public struct SetScissorRect : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetScissorRect>();
        public static int SizeInBytes => sizeInBytes;

        public SetScissorRect() { }

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

    public struct SetBlendConstant : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetBlendConstant>();
        public static int SizeInBytes => sizeInBytes;

        public SetBlendConstant() { }

        public readonly RenderCommandType Type = RenderCommandType.SetBlendConstant;
        public required Vec4<double> BlendConstant;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            renderPassEncoder.SetBlendConstant(BlendConstant);
        }
    }

    public struct SetPipeline : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetPipeline>();
        public static int SizeInBytes => sizeInBytes;

        public SetPipeline() { }

        public readonly RenderCommandType Type = RenderCommandType.SetPipeline;
        public required Handle<GPURenderPipeline> Pipeline;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var pipeline = renderAssets.Get(Pipeline);
            renderPassEncoder.SetPipeline(pipeline);
        }
    }

    public struct SetVertexBuffer : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetVertexBuffer>();
        public static int SizeInBytes => sizeInBytes;

        public SetVertexBuffer() { }

        public readonly RenderCommandType Type = RenderCommandType.SetVertexBuffer;
        public required uint Slot;
        public required Handle<GPUBuffer> Buffer;
        public uint? Offset;
        public uint? Length;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(Buffer);
            renderPassEncoder.SetVertexBuffer(Slot, buffer, Offset, Length);
        }
    }

    public struct SetIndexBuffer : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetIndexBuffer>();
        public static int SizeInBytes => sizeInBytes;

        public SetIndexBuffer() { }

        public readonly RenderCommandType Type = RenderCommandType.SetIndexBuffer;
        public required Handle<GPUBuffer> Buffer;
        public required IndexFormat Format;
        public uint? Offset;
        public uint? Length;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(Buffer);
            renderPassEncoder.SetIndexBuffer(buffer, Format, Offset, Length);
        }
    }

    public struct SetBindGroup : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<SetBindGroup>();
        public static int SizeInBytes => sizeInBytes;

        public SetBindGroup() { }

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

    public struct Draw : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<Draw>();
        public static int SizeInBytes => sizeInBytes;

        public Draw() { }

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

    public struct DrawIndexed : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndexed>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndexed() { }

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

    public struct DrawIndirect : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndirect>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndirect() { }

        public readonly RenderCommandType Type = RenderCommandType.DrawIndirect;
        public required Handle<GPUBuffer> IndirectBuffer;
        public uint IndirectOffset;

        public void Apply(GPURenderPassEncoder renderPassEncoder, IRenderAssets renderAssets)
        {
            var buffer = renderAssets.Get(IndirectBuffer);
            renderPassEncoder.DrawIndirect(buffer, IndirectOffset);
        }
    }

    public struct DrawIndexedIndirect : IRenderCommand
    {
        public static readonly int sizeInBytes = Unsafe.SizeOf<DrawIndexedIndirect>();
        public static int SizeInBytes => sizeInBytes;

        public DrawIndexedIndirect() { }

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


public static class RenderCommandsExt
{
    public static ref RenderCommands SetViewport(this ref RenderCommands commands, Vec2f origin, Vec2f size, float minDepth, float maxDepth)
    {
        commands.WriteCommand(new RenderCommands.SetViewport
        {
            Origin = origin,
            Size = size,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
        });
        return ref commands;
    }

    public static ref RenderCommands SetScissorRect(this ref RenderCommands commands, uint x, uint y, uint width, uint height)
    {
        commands.WriteCommand(new RenderCommands.SetScissorRect
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
        });
        return ref commands;
    }

    public static ref RenderCommands SetBlendConstant(this ref RenderCommands commands, Vec4<double> blendConstant)
    {
        commands.WriteCommand(new RenderCommands.SetBlendConstant
        {
            BlendConstant = blendConstant,
        });
        return ref commands;
    }

    public static ref RenderCommands SetPipeline(this ref RenderCommands commands, Handle<GPURenderPipeline> pipeline)
    {
        commands.WriteCommand(new RenderCommands.SetPipeline
        {
            Pipeline = pipeline,
        });
        return ref commands;
    }

    public static ref RenderCommands SetVertexBuffer(this ref RenderCommands commands, uint slot, Handle<GPUBuffer> buffer, uint? offset = null, uint? length = null)
    {
        commands.WriteCommand(new RenderCommands.SetVertexBuffer
        {
            Slot = slot,
            Buffer = buffer,
            Offset = offset,
            Length = length,
        });
        return ref commands;
    }

    public static ref RenderCommands SetIndexBuffer(this ref RenderCommands commands, Handle<GPUBuffer> buffer, IndexFormat format, uint? offset = null, uint? length = null)
    {
        commands.WriteCommand(new RenderCommands.SetIndexBuffer
        {
            Buffer = buffer,
            Format = format,
            Offset = offset,
            Length = length,
        });
        return ref commands;
    }

    public static ref RenderCommands SetBindGroup(this ref RenderCommands commands, uint groupIndex, Handle<GPUBindGroup> bindGroup, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
    {
        commands.WriteCommand(new RenderCommands.SetBindGroup
        {
            GroupIndex = groupIndex,
            BindGroup = bindGroup,
            DynamicOffsetCount = dynamicOffsetCount,
            DynamicOffsets = dynamicOffsets,
        });
        return ref commands;
    }

    public static ref RenderCommands Draw(this ref RenderCommands commands, uint vertexCount, uint instanceCount, uint vertexOffset = 0, uint instanceOffset = 0)
    {
        commands.WriteCommand(new RenderCommands.Draw
        {
            VertexCount = vertexCount,
            InstanceCount = instanceCount,
            VertexOffset = vertexOffset,
            InstanceOffset = instanceOffset,
        });
        return ref commands;
    }

    public static ref RenderCommands DrawIndexed(this ref RenderCommands commands, uint indexCount, uint instanceCount, uint firstIndex = 0, int baseVertex = 0, uint firstInstance = 0)
    {
        commands.WriteCommand(new RenderCommands.DrawIndexed
        {
            IndexCount = indexCount,
            InstanceCount = instanceCount,
            FirstIndex = firstIndex,
            BaseVertex = baseVertex,
            FirstInstance = firstInstance,
        });
        return ref commands;
    }

    public static ref RenderCommands DrawIndirect(this ref RenderCommands commands, Handle<GPUBuffer> indirectBuffer, uint indirectOffset)
    {
        commands.WriteCommand(new RenderCommands.DrawIndirect
        {
            IndirectBuffer = indirectBuffer,
            IndirectOffset = indirectOffset,
        });
        return ref commands;
    }

    public static ref RenderCommands DrawIndexedIndirect(this ref RenderCommands commands, Handle<GPUBuffer> indirectBuffer, uint indirectOffset)
    {
        commands.WriteCommand(new RenderCommands.DrawIndexedIndirect
        {
            IndirectBuffer = indirectBuffer,
            IndirectOffset = indirectOffset,
        });
        return ref commands;
    }
}