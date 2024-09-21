using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pollus.Coroutine;

public struct Yield
{
    public enum Instruction
    {
        None = 0,
        Return,
        WaitForSeconds,
    }

    [InlineArray(16)]
    struct InstructionData
    {
        byte _first;
    }

    Instruction instruction;
    InstructionData instructionData;

    public Instruction CurrentInstruction => instruction;

    public Yield(Instruction instruction, ReadOnlySpan<byte> data)
    {
        this.instruction = instruction;
        data.CopyTo(instructionData);
    }

    public T GetData<T>()
        where T : unmanaged
    {
        return MemoryMarshal.Read<T>(instructionData[..Unsafe.SizeOf<T>()]);
    }

    public void SetData<T>(in T value)
        where T : unmanaged
    {
        MemoryMarshal.Write(instructionData[..Unsafe.SizeOf<T>()], value);
    }

    public static Yield Return() => new(Instruction.Return, ReadOnlySpan<byte>.Empty);
    public static Yield WaitForSeconds(float seconds) => new(Instruction.WaitForSeconds, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref seconds, 1)));
}