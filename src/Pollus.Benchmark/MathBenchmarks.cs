namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Pollus.Mathematics;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
// [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class MathBenchmarks
{
    [Benchmark]
    public Mat4f Mat4f_Multiply()
    {
        var left = Mat4f.Identity();
        for (int i = 0; i < 1000; i++)
        {
            left = left * Mat4f.Identity();
        }
        return left;
    }

    [Benchmark]
    public System.Numerics.Matrix4x4 System_Numerics_Matrix4x4_Multiply()
    {
        var left = System.Numerics.Matrix4x4.Identity;
        for (int i = 0; i < 1000; i++)
        {
            left = System.Numerics.Matrix4x4.Multiply(left, System.Numerics.Matrix4x4.Identity);
        }
        return left;
    }

    [Benchmark]
    public Silk.NET.Maths.Matrix4X4<float> Silk_NET_Maths_Matrix4X4_Multiply()
    {
        var left = Silk.NET.Maths.Matrix4X4<float>.Identity;
        for (int i = 0; i < 1000; i++)
        {
            left = Silk.NET.Maths.Matrix4X4.Multiply(left, Silk.NET.Maths.Matrix4X4<float>.Identity);
        }
        return left;
    }

    [Benchmark]
    public Vec4f Vec4f_Multiply()
    {
        var left = Vec4f.One;
        for (int i = 0; i < 1000; i++)
        {
            left = left * Vec4f.One;
        }
        return left;
    }

    [Benchmark]
    public System.Numerics.Vector4 System_Numerics_Vector4_Multiply()
    {
        var left = System.Numerics.Vector4.One;
        for (int i = 0; i < 1000; i++)
        {
            left = left * System.Numerics.Vector4.One;
        }
        return left;
    }

    [Benchmark]
    public Silk.NET.Maths.Vector4D<float> Silk_NET_Maths_Vector4D_Multiply()
    {
        var left = Silk.NET.Maths.Vector4D<float>.One;
        for (int i = 0; i < 1000; i++)
        {
            left = left * Silk.NET.Maths.Vector4D<float>.One;
        }
        return left;
    }
}
