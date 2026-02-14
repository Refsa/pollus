namespace Pollus.Tests;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Platform;

public class ApplicationLifecycleTests
{
    class BlockingRunner : IAppRunner
    {
        public bool IsBlocking => true;
        public bool SetupCalled;
        public bool RunCalled;

        public void Setup(World world) => SetupCalled = true;

        public void Run(World world, Func<bool> isRunning, Action requestShutdown)
        {
            RunCalled = true;
            // Simulate a few update ticks then exit
            for (int i = 0; i < 3 && isRunning(); i++)
                world.Update();
        }
    }

    class NonBlockingRunner : IAppRunner
    {
        public bool IsBlocking => false;
        public bool SetupCalled;
        public bool RunCalled;

        public void Setup(World world) => SetupCalled = true;

        public void Run(World world, Func<bool> isRunning, Action requestShutdown)
        {
            RunCalled = true;
            // Non-blocking: return immediately, app stays alive
        }
    }

    class ThrowingSetupRunner : IAppRunner
    {
        public bool IsBlocking => true;
        public bool SetupCalled;
        public bool RunCalled;

        public void Setup(World world)
        {
            SetupCalled = true;
            throw new InvalidOperationException("Setup failed");
        }

        public void Run(World world, Func<bool> isRunning, Action requestShutdown)
        {
            RunCalled = true;
        }
    }

    [Fact]
    public void BlockingRunner_Run_DisposesOnReturn()
    {
        var runner = new BlockingRunner();
        var app = new ApplicationBuilder()
            .WithRunner(runner)
            .Build();

        app.Run();

        Assert.True(runner.SetupCalled);
        Assert.True(runner.RunCalled);
        Assert.False(app.IsRunning);
    }

    [Fact]
    public void NonBlockingRunner_Run_StaysAliveUntilShutdown()
    {
        var runner = new NonBlockingRunner();
        var app = new ApplicationBuilder()
            .WithRunner(runner)
            .Build();

        app.Run();

        Assert.True(runner.SetupCalled);
        Assert.True(runner.RunCalled);
        Assert.True(app.IsRunning);

        app.Shutdown();

        Assert.False(app.IsRunning);
    }

    [Fact]
    public void SetupThrows_DisposesWorldAndRethrows()
    {
        var runner = new ThrowingSetupRunner();
        var app = new ApplicationBuilder()
            .WithRunner(runner)
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() => app.Run());

        Assert.Equal("Setup failed", ex.Message);
        Assert.True(runner.SetupCalled);
        Assert.False(runner.RunCalled);
        Assert.False(app.IsRunning);
    }
}
