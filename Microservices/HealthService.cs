namespace Microservices;

/// <summary>
/// Микросервис проверки здоровья.
/// </summary>
public sealed class HealthService : IMicroservice
{
    public string Name => "HealthService";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[HealthService] Здоровье в норме. Сервис работает.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[HealthService] Остановлен.");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(float deltaTime)
    {
        return Task.CompletedTask;
    }
}