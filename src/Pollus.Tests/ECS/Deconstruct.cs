/* namespace Pollus.Tests.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS;

struct Test<C0, C1>
    where C0 : unmanaged
    where C1 : unmanaged
{
    public C0[] c0;
    public C1[] c1;

    public Test(C0[] c0, C1[] c1)
    {
        this.c0 = c0;
        this.c1 = c1;
    }

    public Enumerator GetEnumerator() => new(c0, c1);

    public ref struct Enumerator
    {
        int index = -1;
        Span<C0> c0;
        Span<C1> c1;
        Item current;

        public Enumerator(Span<C0> c0, Span<C1> c1)
        {
            this.c0 = c0;
            this.c1 = c1;
            current = new();
        }

        public ref Item Current => ref Unsafe.AsRef(ref current);

        public bool MoveNext()
        {
            if (++index < c0.Length) 
            {
                current.Item1 = ref c0[index];
                current.Item2 = ref c1[index];
                return true;
            }
            return false;
        }
    }

    public ref struct Item
    {
        public ref C0 Item1;
        public ref C1 Item2;

        public Item(ref C0 c0, ref C1 c1)
        {
            Item1 = ref c0;
            Item2 = ref c1;
        }
    }
}

public class Deconstruct
{
    [Fact]
    public void Test()
    {
        var test = new Test<int, int>([1, 2, 3], [4, 5, 6]);
        int index = 0;
        foreach (ref var tuple in test)
        {
            ref var c0 = ref tuple.Item1;
            ref var c1 = ref tuple.Item2;
        
            Assert.Equal(test.c0[index], c0);
            Assert.Equal(test.c1[index], c1);
            c0 += c1;
            index++;
        }

        Assert.Equal([5, 7, 9], test.c0);
        Assert.Equal([4, 5, 6], test.c1);
    }
} */