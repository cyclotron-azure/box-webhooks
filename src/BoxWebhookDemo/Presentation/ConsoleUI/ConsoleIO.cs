namespace BoxWebhookDemo.Presentation.ConsoleUI;

/// <summary>
/// Standard console I/O implementation.
/// </summary>
public class ConsoleIO : IConsoleIO
{
    public void WriteLine(string message) => Console.WriteLine(message);
    public void Write(string message) => Console.Write(message);
    public string? ReadLine() => Console.ReadLine();
    public void Clear() => Console.Clear();
}
