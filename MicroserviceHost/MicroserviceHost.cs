namespace Microservices;

/// <summary>
/// Хост для запуска нескольких микросервисов.
/// </summary>
public sealed class MicroserviceHost
{
    /// <summary>
    /// Все зарегистрированные микросервисы (только для чтения).
    /// </summary>
    public IReadOnlyList<IMicroservice> Services { get; }

    public MicroserviceHost(IEnumerable<IMicroservice> services)
    {
        Services = services?.ToList() ?? [];
    }

    /// <summary>
    /// Запускает все микросервисы.
    /// </summary>
    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        await LoggingService.LogAsync("=== Запуск микросервисов ===");

        foreach (var service in Services)
        {
            await LoggingService.LogAsync($"[Host] Запуск {service.Name}...");
            await service.StartAsync(cancellationToken);
        }

        await LoggingService.LogAsync("=== Все микросервисы запущены ===");
    }

    /// <summary>
    /// Останавливает все микросервисы.
    /// </summary>
    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        await LoggingService.LogAsync("=== Остановка микросервисов ===");

        // Останавливаем в обратном порядке
        for (int i = Services.Count - 1; i >= 0; i--)
        {
            var service = Services[i];
            await LoggingService.LogAsync($"[Host] Остановка {service.Name}...");
            await service.StopAsync(cancellationToken);
        }

        await LoggingService.LogAsync("=== Все микросервисы остановлены ===");
    }

    /// <summary>
    /// Запускает цикл обновления для всех микросервисов.
    /// Вызывает UpdateAsync у каждого сервиса с указанным интервалом.
    /// </summary>
    /// <param name="intervalMs">Интервал между вызовами в миллисекундах (по умолчанию 1000 = 1 сек).</param>
    /// <param name="cancellationToken">Токен отмены для остановки цикла.</param>
    public async Task RunUpdateLoopAsync(int intervalMs = 1000, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var startTime = DateTime.UtcNow;

            foreach (var service in Services)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await service.UpdateAsync(intervalMs / 1000f);
            }

            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var delay = (int)Math.Max(0, intervalMs - elapsed);

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}