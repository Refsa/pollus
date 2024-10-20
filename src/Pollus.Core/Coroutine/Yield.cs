namespace Pollus.Coroutine;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

public struct Yield : IDisposable
{
    public enum Type
    {
        None = 0,
        Return,
        WaitForSeconds,
        Custom,
    }

    Type instruction;
    byte[]? instructionData;

    public Type Instruction => instruction;

    public Yield(Type instruction, int capacity)
    {
        if (capacity > 0) instructionData = ArrayPool<byte>.Shared.Rent(capacity);
        this.instruction = instruction;
    }

    public void Dispose()
    {
        if (instructionData is not null) ArrayPool<byte>.Shared.Return(instructionData);
    }

    public T GetData<T>(int offset = 0)
        where T : unmanaged
    {
        return MemoryMarshal.Read<T>(instructionData.AsSpan()[offset..(offset + Unsafe.SizeOf<T>())]);
    }

    public TCustomData GetCustomData<TCustomData>()
        where TCustomData : struct
    {
        return MemoryMarshal.Read<TCustomData>(instructionData.AsSpan()[4..]);
    }

    public void SetData<T>(in T value)
        where T : unmanaged
    {
        MemoryMarshal.Write(instructionData.AsSpan()[..Unsafe.SizeOf<T>()], value);
    }

    public static Yield Return => new(Type.Return, 0);

    public static Yield WaitForSeconds(float seconds)
    {
        var yield = new Yield(Type.WaitForSeconds, Unsafe.SizeOf<float>());
        MemoryMarshal.Write(yield.instructionData, seconds);
        return yield;
    }

    public static Yield Custom<TCustomData>(in TCustomData data)
        where TCustomData : struct
    {
        var yield = new Yield(Type.Custom, Unsafe.SizeOf<YieldCustomData<TCustomData>>());
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

    public delegate bool HandlerDelegate(in Yield yield, in TParam param);
    static Dictionary<int, HandlerData> handlers = [];

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