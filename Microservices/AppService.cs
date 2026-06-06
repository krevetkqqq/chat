namespace Microservices;

/// <summary>
/// Главный веб-микросервис приложения.
/// Регистрирует API-эндпоинты и управляет веб-интерфейсом.
/// </summary>
public sealed class AppService : IWebMicroservice
{
    public string Name => "AppService";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[AppService] Веб запущен.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[AppService] Веб остановлен.");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(float deltaTime)
    {
        return Task.CompletedTask;
    }

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/info", (MicroserviceHost host) =>
            new
            {
                Status = "running",
                Services = host.Services.Select(s => s.Name)
            });
    }
}