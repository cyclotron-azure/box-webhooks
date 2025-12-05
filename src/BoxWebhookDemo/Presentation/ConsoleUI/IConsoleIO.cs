namespace BoxWebhookDemo.Presentation.ConsoleUI;

/// <summary>
/// Abstraction for console I/O operations.
/// Follows Dependency Inversion principle (DIP) - depend on abstractions.
/// Enables unit testing of console-based code.
/// </summary>
public interface IConsoleIO
{
    void WriteLine(string message);
    void Write(string message);
    string? ReadLine();
    void Clear();
}
