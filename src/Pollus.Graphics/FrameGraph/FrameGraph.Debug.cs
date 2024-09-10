namespace Pollus.Graphics;

public partial struct FrameGraph<TParam>
{
    /// <summary>
    /// Visualize the frame graph in DOT format
    /// </summary>
    public string Visualize()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("  node [style=filled];");
        foreach (var node in passNodes.Nodes)
        {
            sb.AppendLine($"  pn{node.Index} [label=\"{node.Name}\", shape=box, color=\"#FFFF00\", fillcolor=\"#FFFF00B3\"];");
        }

        foreach (var kvp in resources.ResourceByName)
        {
            sb.AppendLine($"  r{kvp.Value.Id} [label=\"{kvp.Key}\", shape=box, color=\"#0000FF\", fillcolor=\"#0000FFB3\"];");
        }

        foreach (var node in passNodes.Nodes)
        {
            foreach (var write in node.Writes)
            {
                sb.AppendLine($"  pn{node.Index} -> r{write};");
            }

            foreach (var read in node.Reads)
            {
                sb.AppendLine($"  r{read} -> pn{node.Index};");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}