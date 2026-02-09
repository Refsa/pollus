namespace Pollus.Graphics;

using System.Buffers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public enum ComputeCommandType : ushort
{
    Undefined = 0,
    SetPipeline,
    SetBindGroup,
    Dispatch,
}

public interface IComputeCommand
{
    static abstract int SizeInBytes { get; }

    void Apply(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets);
}

public struct ComputeCommands : IDisposable
{
    byte[] commands;
    int cursor;
    int count;

    public int Count => count;

    public ComputeCommands()
    {
        commands = ArrayPool<byte>.Shared.Rent(256);
    }

    public static ComputeCommands Builder => new();

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

    void EnsureSize<TCommand>() where TCommand : unmanaged, IComputeCommand
    {
        if (cursor + TCommand.SizeInBytes > commands.Length)
            Resize();
    }

    TCommand ReadCommand<TCommand>(ref int offset)
        where TCommand : unmanaged, IComputeCommand
    {
        TCommand command = Unsafe.ReadUnaligned<TCommand>(ref commands[offset]);
        offset += TCommand.SizeInBytes;
        return command;
    }

    public void WriteCommand<TCommand>(in TCommand command) where TCommand : unmanaged, IComputeCommand
    {
        EnsureSize<TCommand>();
        MemoryMarshal.Write(commands.AsSpan(cursor), in command);
        cursor += TCommand.SizeInBytes;
        count++;
    }

    public void Apply(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets)
    {
        int offset = 0;
        while (offset < cursor)
        {
            ComputeCommandType type = Unsafe.ReadUnaligned<ComputeCommandType>(ref commands[offset]);
            switch (type)
            {
                case ComputeCommandType.SetPipeline:
                    SetPipelineCommand pipelineCommand = ReadCommand<SetPipelineCommand>(ref offset);
                    pipelineCommand.Apply(computePassEncoder, renderAssets);
                    break;
                case ComputeCommandType.SetBindGroup:
                    SetBindGroupCommand bindGroupCommand = ReadCommand<SetBindGroupCommand>(ref offset);
                    bindGroupCommand.Apply(computePassEncoder, renderAssets);
                    break;
                case ComputeCommandType.Dispatch:
                    DispatchCommand dispatchCommand = ReadCommand<DispatchCommand>(ref offset);
                    dispatchCommand.Apply(computePassEncoder, renderAssets);
                    break;
                default:
                    throw new NotImplementedException($"Unknown command type: {type}");
            }
        }
    }

    public void ApplyAndDispose(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets)
    {
        Apply(computePassEncoder, renderAssets);
        Dispose();
    }

    public ComputeCommands SetPipeline(Handle<GPUComputePipeline> pipeline)
    {
        WriteCommand(new SetPipelineCommand { Pipeline = pipeline });
        return this;
    }

    public ComputeCommands SetBindGroup(uint index, Handle<GPUBindGroup> bindGroup)
    {
        WriteCommand(new SetBindGroupCommand { BindGroup = bindGroup, Index = index });
        return this;
    }

    public ComputeCommands SetBindGroups(uint startIndex, ReadOnlySpan<Handle<GPUBindGroup>> bindGroups)
    {
        for (int i = 0; i < bindGroups.Length; i++)
        {
            SetBindGroup(startIndex + (uint)i, bindGroups[i]);
        }
        return this;
    }

    public ComputeCommands Dispatch(uint x, uint y, uint z)
    {
        WriteCommand(new DispatchCommand { X = x, Y = y, Z = z });
        return this;
    }

    public struct SetPipelineCommand : IComputeCommand
    {
        public static int SizeInBytes => Unsafe.SizeOf<SetPipelineCommand>();

        public ComputeCommandType Type = ComputeCommandType.SetPipeline;
        public required Handle<GPUComputePipeline> Pipeline;

        public SetPipelineCommand() { }

        public void Apply(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets)
        {
            computePassEncoder.SetPipeline(renderAssets.Get(Pipeline));
        }
    }

    public struct SetBindGroupCommand : IComputeCommand
    {
        public static int SizeInBytes => Unsafe.SizeOf<SetBindGroupCommand>();

        public ComputeCommandType Type = ComputeCommandType.SetBindGroup;
        public required Handle<GPUBindGroup> BindGroup;
        public required uint Index;

        public SetBindGroupCommand() { }

        public void Apply(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets)
        {
            computePassEncoder.SetBindGroup(Index, renderAssets.Get(BindGroup));
        }
    }

    public struct DispatchCommand : IComputeCommand
    {
        public static int SizeInBytes => Unsafe.SizeOf<DispatchCommand>();

        public ComputeCommandType Type = ComputeCommandType.Dispatch;
        public uint X;
        public uint Y;
        public uint Z;

        public DispatchCommand() { }

        public void Apply(GPUComputePassEncoder computePassEncoder, IRenderAssets renderAssets)
        {
            computePassEncoder.Dispatch(X, Y, Z);
        }
    }
}

