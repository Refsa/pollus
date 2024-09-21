namespace Pollus.Coroutine;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        MemoryMarshal.Write(yield.instructionData, data);
        return yield;
    }
}

public struct YieldCustomData<TInstruction, TData>
    where TInstruction : unmanaged
    where TData : unmanaged
{
    public required TInstruction Instruction;
    public required int TypeID;
    public required TData Data;
}

public static class YieldCustomInstructionHandler<TInstruction, TParam>
    where TInstruction : unmanaged
    where TParam : struct
{
    public delegate bool HandlerDelegate(in Yield yield, TParam param);
    static Dictionary<int, HandlerDelegate> handlers = new();
    public static void AddHandler(int typeId, HandlerDelegate handler)
    {
        handlers[typeId] = handler;
    }

    public static bool Handle(in Yield yield, TParam param)
    {
        var instruction = yield.GetData<TInstruction>(0);
        var typeId = yield.GetData<int>(Unsafe.SizeOf<TInstruction>());
        return handlers[typeId](in yield, param);
    }
}