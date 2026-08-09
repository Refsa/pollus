namespace Pollus.Tests.Graphics;

using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

[ShaderType]
public partial struct ScalarThenVector
{
    public float Scalar;
    public Vec2f Vector;
}

[ShaderType]
public partial struct VectorThenScalar
{
    public Vec2f Vector;
    public float Scalar;
}

[ShaderType]
public partial struct NestedShaderType
{
    public VectorThenScalar Nested;
    public float Scalar;
}

[ShaderType]
public partial struct ThreeScalars
{
    public float A;
    public float B;
    public float C;
}

public class ShaderTypeTests
{
    [Fact]
    public void CountsElementSizesWithoutKnownTypeRules()
    {
        Assert.Equal(ThreeScalars.SizeOf, Vec3f.SizeOf);
        Assert.Equal(ThreeScalars.AlignOf, Vec3f.AlignOf);
        Assert.Equal(ThreeScalars.PaddedSize, Vec3f.PaddedSize);
    }

    [Fact]
    public void CountsElementsWithoutInsertingOffsets()
    {
        Assert.Equal(12u, ScalarThenVector.SizeOf);
        Assert.Equal(16u, ScalarThenVector.AlignOf);
        Assert.Equal(16u, ScalarThenVector.PaddedSize);
    }

    [Fact]
    public void ReportsPaddedSize()
    {
        Assert.Equal(12u, VectorThenScalar.SizeOf);
        Assert.Equal(16u, VectorThenScalar.AlignOf);
        Assert.Equal(16u, VectorThenScalar.PaddedSize);
    }

    [Fact]
    public void CountsNestedElementSizes()
    {
        Assert.Equal(16u, NestedShaderType.SizeOf);
        Assert.Equal(16u, NestedShaderType.AlignOf);
        Assert.Equal(16u, NestedShaderType.PaddedSize);
    }

    [Fact]
    public void CountsMatrixElementSizes()
    {
        Assert.Equal(36u, Mat3f.SizeOf);
        Assert.Equal(16u, Mat3f.AlignOf);
        Assert.Equal(48u, Mat3f.PaddedSize);
    }

    [Fact]
    public void UniformAcceptsElementCountedHostLayout()
    {
        var uniform = new Uniform<ScalarThenVector>();
        var descriptor = BufferDescriptor.Uniform<ScalarThenVector>(nameof(ScalarThenVector));

        Assert.Equal(16ul, uniform.SizeInBytes);
        Assert.Equal(16ul, descriptor.Size);
    }

    [Fact]
    public void StorageBufferRejectsHostStrideMismatch()
    {
        Assert.Throws<InvalidOperationException>(() => StorageBuffer.From<VectorThenScalar>(1, BufferUsage.CopyDst));
    }

}
