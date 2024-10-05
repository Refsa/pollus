namespace Pollus.Graphics.Rendering;

[ShaderType]
public partial struct IndirectBufferData
{
    public uint VertexCount;
    public uint InstanceCount;
    public uint FirstVertex;
    public uint FirstInstance;
}