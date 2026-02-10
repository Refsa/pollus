using Pollus.UI.Layout;

namespace Pollus.Tests.UI.Layout;

public class LayoutCacheTests
{
    static LayoutInput MakeInput(RunMode mode = RunMode.ComputeSize,
        float? knownW = null, float? knownH = null,
        float availW = 100f, float availH = 100f)
    {
        return new LayoutInput
        {
            RunMode = mode,
            KnownDimensions = new Size<float?>(knownW, knownH),
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.Definite(availW),
                AvailableSpace.Definite(availH)),
        };
    }

    static LayoutOutput MakeOutput(float w, float h)
    {
        return new LayoutOutput { Size = new Size<float>(w, h) };
    }

    [Fact]
    public void EmptyCache_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input = MakeInput();
        Assert.False(cache.TryGet(in input, out _));
    }

    [Fact]
    public void StoreAndGet_SameInput_ReturnsCached()
    {
        var cache = new LayoutCache();
        var input = MakeInput(RunMode.ComputeSize, 50f, 50f);
        var output = MakeOutput(50f, 50f);

        cache.Store(in input, in output);
        Assert.True(cache.TryGet(in input, out var cached));
        Assert.Equal(output.Size.Width, cached.Size.Width);
        Assert.Equal(output.Size.Height, cached.Size.Height);
    }

    [Fact]
    public void Get_DifferentRunMode_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input1 = MakeInput(RunMode.ComputeSize, 50f, 50f);
        var input2 = MakeInput(RunMode.PerformLayout, 50f, 50f);
        var output = MakeOutput(50f, 50f);

        cache.Store(in input1, in output);
        Assert.False(cache.TryGet(in input2, out _));
    }

    [Fact]
    public void Get_DifferentKnownDimensions_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input1 = MakeInput(knownW: 50f, knownH: 50f);
        var input2 = MakeInput(knownW: 100f, knownH: 50f);
        var output = MakeOutput(50f, 50f);

        cache.Store(in input1, in output);
        Assert.False(cache.TryGet(in input2, out _));
    }

    [Fact]
    public void Get_NullVsDefiniteKnownDimension_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input1 = MakeInput(knownW: null, knownH: 50f);
        var input2 = MakeInput(knownW: 50f, knownH: 50f);
        var output = MakeOutput(50f, 50f);

        cache.Store(in input1, in output);
        Assert.False(cache.TryGet(in input2, out _));
    }

    [Fact]
    public void Get_DifferentAvailableSpace_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input1 = MakeInput(availW: 100f);
        var input2 = MakeInput(availW: 200f);
        var output = MakeOutput(100f, 100f);

        cache.Store(in input1, in output);
        Assert.False(cache.TryGet(in input2, out _));
    }

    [Fact]
    public void Get_DifferentAvailableSpaceKind_ReturnsFalse()
    {
        var cache = new LayoutCache();
        var input1 = MakeInput();
        var input2 = new LayoutInput
        {
            RunMode = RunMode.ComputeSize,
            KnownDimensions = input1.KnownDimensions,
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.MaxContent,
                AvailableSpace.Definite(100f)),
        };
        var output = MakeOutput(100f, 100f);

        cache.Store(in input1, in output);
        Assert.False(cache.TryGet(in input2, out _));
    }

    [Fact]
    public void Store_MultipleEntries_AllRetrievable()
    {
        var cache = new LayoutCache();

        for (int i = 0; i < 9; i++)
        {
            var input = MakeInput(availW: i * 10f);
            var output = MakeOutput(i * 10f, 100f);
            cache.Store(in input, in output);
        }

        for (int i = 0; i < 9; i++)
        {
            var input = MakeInput(availW: i * 10f);
            Assert.True(cache.TryGet(in input, out var cached));
            Assert.Equal(i * 10f, cached.Size.Width);
        }
    }

    [Fact]
    public void Store_OverNineEntries_EvictsOldest()
    {
        var cache = new LayoutCache();

        // Fill all 9 slots
        for (int i = 0; i < 9; i++)
        {
            var input = MakeInput(availW: i * 10f);
            var output = MakeOutput(i * 10f, 100f);
            cache.Store(in input, in output);
        }

        // Store 10th entry - should evict slot 0 (availW = 0)
        var newInput = MakeInput(availW: 999f);
        var newOutput = MakeOutput(999f, 100f);
        cache.Store(in newInput, in newOutput);

        // First entry should be evicted
        var evictedInput = MakeInput(availW: 0f);
        Assert.False(cache.TryGet(in evictedInput, out _));

        // New entry should be present
        Assert.True(cache.TryGet(in newInput, out var cached));
        Assert.Equal(999f, cached.Size.Width);

        // Other entries still present
        for (int i = 1; i < 9; i++)
        {
            var input = MakeInput(availW: i * 10f);
            Assert.True(cache.TryGet(in input, out _));
        }
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new LayoutCache();

        for (int i = 0; i < 5; i++)
        {
            var input = MakeInput(availW: i * 10f);
            var output = MakeOutput(i * 10f, 100f);
            cache.Store(in input, in output);
        }

        cache.Clear();

        for (int i = 0; i < 5; i++)
        {
            var input = MakeInput(availW: i * 10f);
            Assert.False(cache.TryGet(in input, out _));
        }
    }

    [Fact]
    public void Clear_ResetsInsertionPointer()
    {
        var cache = new LayoutCache();

        // Fill 5 slots
        for (int i = 0; i < 5; i++)
        {
            var input = MakeInput(availW: i * 10f);
            var output = MakeOutput(i * 10f, 100f);
            cache.Store(in input, in output);
        }

        cache.Clear();

        // Fill all 9 slots again - should work without eviction
        for (int i = 0; i < 9; i++)
        {
            var input = MakeInput(availW: (i + 100) * 10f);
            var output = MakeOutput((i + 100) * 10f, 100f);
            cache.Store(in input, in output);
        }

        for (int i = 0; i < 9; i++)
        {
            var input = MakeInput(availW: (i + 100) * 10f);
            Assert.True(cache.TryGet(in input, out _));
        }
    }

    [Fact]
    public void Store_PreservesContentSizeAndBaselines()
    {
        var cache = new LayoutCache();
        var input = MakeInput();
        var output = new LayoutOutput
        {
            Size = new Size<float>(100f, 50f),
            ContentSize = new Size<float>(80f, 40f),
            FirstBaselines = new Point<float?>(12f, null),
        };

        cache.Store(in input, in output);
        Assert.True(cache.TryGet(in input, out var cached));
        Assert.Equal(80f, cached.ContentSize.Width);
        Assert.Equal(40f, cached.ContentSize.Height);
        Assert.Equal(12f, cached.FirstBaselines.X);
        Assert.Null(cached.FirstBaselines.Y);
    }
}
