using System.Runtime.CompilerServices;
using Pollus.Mathematics;

namespace Pollus.Spatial;

public class SphereQuadTree<TData>
    where TData : struct
{
    public struct Node
    {
        public Vec2f Center;
        public float Radius;
        public TData Data;
        public ChildrenArray Children;
        public bool IsLeaf;

        public Node(Vec2f center, float radius, TData data)
        {
            Center = center;
            Radius = radius;
            Data = data;
            IsLeaf = true;
            for (int i = 0; i < 4; i++) Children[i] = -1;
        }

        [InlineArray(4)]
        public struct ChildrenArray
        {
            int first;
        }
    }

    Node[] nodes;
    int root;
    int capacity;
    readonly float maxRadius;
    readonly int maxDepth;

    public SphereQuadTree(Vec2f center, float radius, float maxRadius, int maxDepth, int initialCapacity = 1024)
    {
        nodes = new Node[initialCapacity];
        nodes[0] = new Node(center, radius, default(TData));
        root = 0;
        capacity = 1;
        this.maxRadius = maxRadius;
        this.maxDepth = maxDepth;
    }

    public void Insert(Vec2f position, float radius, TData data)
    {
        InsertRecursive(root, position, radius, data, 0);
    }

    void InsertRecursive(int nodeIndex, Vec2f position, float radius, TData data, int depth)
    {
        ref Node currentNode = ref nodes[nodeIndex];
        if (currentNode.IsLeaf)
        {
            if (depth < maxDepth && currentNode.Radius / 2 >= maxRadius)
            {
                Subdivide(ref currentNode);
                int quadrant = GetQuadrant(currentNode, position);
                if (currentNode.Children[quadrant] == -1)
                {
                    currentNode.Children[quadrant] = AddNode(new Node(GetChildCenter(currentNode, quadrant), currentNode.Radius / 2, default(TData)));
                }
                InsertRecursive(currentNode.Children[quadrant], position, radius, data, depth + 1);
            }
            else
            {
                currentNode.Data = data;
            }
        }
        else
        {
            int quadrant = GetQuadrant(currentNode, position);
            if (currentNode.Children[quadrant] == -1)
            {
                currentNode.Children[quadrant] = AddNode(new Node(GetChildCenter(currentNode, quadrant), currentNode.Radius / 2, default(TData)));
            }
            InsertRecursive(currentNode.Children[quadrant], position, radius, data, depth + 1);
        }
    }

    void Subdivide(ref Node node)
    {
        node.IsLeaf = false;
    }

    Vec2f GetChildCenter(Node parent, int quadrant)
    {
        float offset = parent.Radius / 2;
        return quadrant switch
        {
            0 => new Vec2f(parent.Center.X - offset, parent.Center.Y - offset),
            1 => new Vec2f(parent.Center.X + offset, parent.Center.Y - offset),
            2 => new Vec2f(parent.Center.X - offset, parent.Center.Y + offset),
            3 => new Vec2f(parent.Center.X + offset, parent.Center.Y + offset),
            _ => parent.Center
        };
    }

    int GetQuadrant(Node node, Vec2f position)
    {
        int quadrant = 0;
        if (position.X >= node.Center.X) quadrant += 1;
        if (position.Y >= node.Center.Y) quadrant += 2;
        return quadrant;
    }

    int AddNode(Node node)
    {
        if (capacity == nodes.Length)
        {
            Array.Resize(ref nodes, nodes.Length * 2);
        }
        nodes[capacity] = node;
        return capacity++;
    }

    public int Query(Vec2f position, float radius, Span<TData> result)
    {
        int resultCursor = 0;
        QueryRecursive(root, position, radius, result, ref resultCursor);
        return resultCursor;
    }

    void QueryRecursive(int nodeIndex, Vec2f position, float radius, Span<TData> result, ref int resultCursor)
    {
        if (resultCursor >= result.Length) return;

        ref Node node = ref nodes[nodeIndex];
        float distSquared = (node.Center - position).LengthSquared();
        float totalRadius = node.Radius + radius;
        if (distSquared > totalRadius * totalRadius)
            return;

        if (node.IsLeaf)
        {
            if (!EqualityComparer<TData>.Default.Equals(node.Data, default(TData)))
            {
                result[resultCursor++] = node.Data;
                if (resultCursor >= result.Length) return;
            }
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (node.Children[i] != -1)
            {
                QueryRecursive(node.Children[i], position, radius, result, ref resultCursor);
            }
        }
    }
}