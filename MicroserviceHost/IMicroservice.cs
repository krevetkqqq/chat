namespace Microservices;

/// <summary>
/// Базовый интерфейс для всех микросервисов.
/// Каждый микросервис должен реализовать этот интерфейс.
/// </summary>
public interface IMicroservice
{
    /// <summary>
    /// Название микросервиса (для логирования).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Вызывается при запуске хоста.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Вызывается при остановке хоста.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Вызывается каждый кадр/тик (например, раз в секунду).
    /// </summary>
    /// <param name="deltaTime">Время, прошедшее с предыдущего вызова (в секундах).</param>
    Task UpdateAsync(float deltaTime);
}

/// <summary>
/// Опциональный интерфейс для микросервисов, которые хотят
/// зарегистрировать свои веб-эндпоинты.
/// </summary>
public interface IWebMicroservice : IMicroservice
{
    /// <summary>
    /// Вызывается при настройке веб-приложения.
    /// Здесь микросервис может зарегистрировать свои MapGet/MapPost и т.д.
    /// </summary>
    void MapEndpoints(WebApplication app);
}