namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Mathematics;

public class KdTree<T>
{
    public struct Node
    {
        public T Data;
        public Vec2f Point;
        public float Radius;
        public int Left;
        public int Right;
        public uint LayerMask;

        public Node(T data, Vec2f point, float radius, uint layerMask)
        {
            Data = data;
            Point = point;
            Radius = radius;
            LayerMask = layerMask;
            Left = -1;
            Right = -1;
        }
    }

    Node[] nodes;
    int size;
    int capacity;

    public KdTree(int initialCapacity = 16)
    {
        capacity = initialCapacity;
        nodes = new Node[capacity];
        size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        size = 0;
    }

    public void Insert(T data, Vec2f point, float radius, uint layerMask)
    {
        if (size == capacity) Resize();

        if (size == 0)
        {
            SetNode(ref nodes[size++], data, point, radius, layerMask);
            return;
        }

        var depth = 0;
        ref var currentNode = ref nodes[0];

        while (true)
        {
            var axis = depth % 2;
            var comparison = axis == 0 ? point.X - currentNode.Point.X : point.Y - currentNode.Point.Y;

            if (comparison < 0)
            {
                if (currentNode.Left == -1)
                {
                    SetNode(ref nodes[size], data, point, radius, layerMask);
                    currentNode.Left = size;
                    size++;
                    return;
                }
                currentNode = ref nodes[currentNode.Left];
            }
            else
            {
                if (currentNode.Right == -1)
                {
                    SetNode(ref nodes[size], data, point, radius, layerMask);
                    currentNode.Right = size;
                    size++;
                    return;
                }
                currentNode = ref nodes[currentNode.Right];
            }
            depth++;
        }

        static void SetNode(ref Node node, T data, Vec2f point, float radius, uint layerMask)
        {
            node.Data = data;
            node.Point = point;
            node.Radius = radius;
            node.LayerMask = layerMask;
            node.Left = -1;
            node.Right = -1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Resize()
    {
        capacity *= 2;
        Array.Resize(ref nodes, capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int RangeSearch(Vec2f target, float range, uint layerMask, ArrayList<T> results)
    {
        RangeSearch(0, target, range, range * range, 0, layerMask, results);
        return results.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RangeSearch(int nodeIndex, Vec2f target, float range, float rangeSqr, int depth, uint layerMask, ArrayList<T> results)
    {
        if (nodeIndex == -1 || nodeIndex >= size)
            return;

        ref var node = ref nodes[nodeIndex];
        if ((node.LayerMask & layerMask) != 0 &&
            (node.Point - target).LengthSquared() <= rangeSqr)
        {
            results.Add(node.Data);
        }

        var axis = depth % 2;
        var diff = axis == 0 ? target.X - node.Point.X : target.Y - node.Point.Y;

        if (diff < 0)
        {
            RangeSearch(node.Left, target, range, rangeSqr, depth + 1, layerMask, results);
            if (Math.Abs(diff) < range)
                RangeSearch(node.Right, target, range, rangeSqr, depth + 1, layerMask, results);
        }
        else
        {
            RangeSearch(node.Right, target, range, rangeSqr, depth + 1, layerMask, results);
            if (Math.Abs(diff) < range)
                RangeSearch(node.Left, target, range, rangeSqr, depth + 1, layerMask, results);
        }
    }
}