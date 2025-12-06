namespace Pollus.Examples;

public interface IExample
{
    string Name { get; }

    void Run();
    void Stop();
}