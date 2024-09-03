namespace Pollus.Graphics;

public interface IShaderType
{
    static abstract uint SizeOf { get; }
    static abstract uint AlignOf { get; }
}

/// <summary>
/// Specify blittable unmanaged type as a host sharable shader type following std430 layout rules.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class ShaderTypeAttribute : Attribute
{

}