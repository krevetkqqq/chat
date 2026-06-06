using System.Text.Json;
using DbsClasses;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;

namespace Microservices;


public sealed class ChatService : IWebMicroservice
{
    private readonly AESClass _aes = new();
    public async Task<string> ReadBody(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        return body;
    }
    private readonly ChatDb _applicationContext = new();
    public string Name => "ChatService";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[ChatService] Веб запущен.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[ChatService] Веб остановлен.");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(float deltaTime)
    {
        return Task.CompletedTask;
    }

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/chats", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[ChatService] GET /chats вызван");

            return Results.Ok(new { message = _applicationContext.GetChats()});
        });
        app.MapGet("/pubkey", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] GET /pubkey вызван");

            return Results.Ok(new { message = RSAClass.GetPublicKey()});
        });
    }
}