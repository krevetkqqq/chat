namespace Microservices;

public sealed class LoggingService
{

    public static Task LogAsync(string message)
    {
        Console.WriteLine($"{DateTime.Now} [LOG] {message}");
        return Task.CompletedTask;
    }
    public static Task DebugAsync(string message)
    {
        Console.WriteLine($"{DateTime.Now} [DEBUG] {message}");
        return Task.CompletedTask;
    }
    public static Task ErrorAsync(string message)
    {
        Console.WriteLine($"{DateTime.Now} [ERROR] {message}");
        return Task.CompletedTask;
    }
    public static Task WarningAsync(string message)
    {
        Console.WriteLine($"{DateTime.Now} [WARNING] {message}");
        return Task.CompletedTask;
    }
}