namespace Pollus.Coroutine;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

public struct Yield
{
    public enum Type
    {
        None = 0,
        Return,
        WaitForSeconds,
        Custom,
    }

    [InlineArray(64)]
    struct InstructionData
    {
        byte _first;
    }

    Type instruction;
    InstructionData instructionData;

    public Type Instruction => instruction;

    public Yield(Type instruction, ReadOnlySpan<byte> data)
    {
        this.instruction = instruction;
        data.CopyTo(instructionData);
    }

    public T GetData<T>(int offset = 0)
        where T : unmanaged
    {
        return MemoryMarshal.Read<T>(instructionData[offset..(offset + Unsafe.SizeOf<T>())]);
    }

    public TCustomData GetCustomData<TCustomData>()
        where TCustomData : struct
    {
        return MemoryMarshal.Read<TCustomData>(instructionData[4..]);
    }

    public void SetData<T>(in T value)
        where T : unmanaged
    {
        MemoryMarshal.Write(instructionData[..Unsafe.SizeOf<T>()], value);
    }

    public static Yield Return() => new(Type.Return, ReadOnlySpan<byte>.Empty);

    public static Yield WaitForSeconds(float seconds)
    {
        var yield = new Yield(Type.WaitForSeconds, ReadOnlySpan<byte>.Empty);
        MemoryMarshal.Write(yield.instructionData, seconds);
        return yield;
    }

    public static Yield Custom<TCustomData>(in TCustomData data)
        where TCustomData : struct
    {
        var yield = new Yield(Type.Custom, ReadOnlySpan<byte>.Empty);
        MemoryMarshal.Write(yield.instructionData, new YieldCustomData<TCustomData>(data));
        return yield;
    }
}

public struct YieldCustomData<TData>
    where TData : struct
{
    public readonly int TypeID = TypeLookup.ID<TData>();
    public TData Data;

    public YieldCustomData(TData data)
    {
        Data = data;
    }
}

public static class YieldCustomInstructionHandler<TParam>
    where TParam : struct
{
    class HandlerData
    {
        public required HandlerDelegate Handler;
        public required Type[] Dependencies;
    }

    public delegate bool HandlerDelegate(in Yield yield, TParam param);
    static Dictionary<int, HandlerData> handlers = new();

    public static void AddHandler<TCustomData>(HandlerDelegate handler, Type[] dependencies)
        where TCustomData : struct
    {
        handlers[TypeLookup.ID<TCustomData>()] = new HandlerData { Handler = handler, Dependencies = dependencies };
    }

    public static bool Handle(in Yield yield, TParam param)
    {
        var typeId = yield.GetData<int>(0);
        return handlers[typeId].Handler(in yield, param);
    }

    public static Type[] GetDependencies(in Yield yield)
    {
        var typeId = yield.GetData<int>(0);
        return handlers[typeId].Dependencies;
    }
}