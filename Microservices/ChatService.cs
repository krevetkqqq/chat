namespace Microservices;

/// <summary>
/// Микросервис чата.
/// </summary>
public sealed class ChatService : IMicroservice
{
    public string Name => "ChatService";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[ChatService] Чат запущен. Ожидание сообщений...");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[ChatService] Чат остановлен.");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(float deltaTime)
    {
        return Task.CompletedTask;
    }
}