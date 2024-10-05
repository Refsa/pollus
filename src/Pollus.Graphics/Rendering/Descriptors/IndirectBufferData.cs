namespace Pollus.Graphics.Rendering;

[ShaderType]
public partial struct IndirectBufferData
{
    public uint InstanceCount;
    public uint VertexCount;
    public uint FirstVertex;
    public uint FirstInstance;
}