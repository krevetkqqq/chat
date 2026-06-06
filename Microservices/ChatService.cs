using System.Text.Json;
using DbsClasses;
using System.Text.Json.Nodes;
using System.Text;
using Newtonsoft.Json;

namespace Microservices;


public sealed class ChatService : IWebMicroservice
{
    private readonly AESClass _aes = new();
    private readonly RSAClass _rsa = new();
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

            return Results.Ok(new { message = _applicationContext.GetChats() });
        });
        app.MapGet("/chat", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[ChatService] GET /chat вызван");
            var body = await ReadBody(context);
            try
            {
                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                if (int.TryParse(chatname, out _))
                    return Results.Ok(new { message = _applicationContext.GetChat(int.Parse(chatname))});
                return Results.Ok(new { message = _applicationContext.GetChat(chatname)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapDelete("/chat", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[ChatService] DELETE /chat вызван");
            var body = await ReadBody(context);
            try
            {
                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                Chat chat;
                if (int.TryParse(chatname, out _))
                    chat = _applicationContext.GetChat(int.Parse(chatname));
                else
                    chat = _applicationContext.GetChat(chatname);
                if (chat.Owner != _aes.Decrypt(token))
                    return Results.BadRequest(new { message = "Only chat owner can delete the chat"});
                return Results.Ok(new { message = _applicationContext.DeleteChat(chat)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapPost("/chat", async (HttpContext context) =>
        {
            try
            {
                await LoggingService.LogAsync("[ChatService] POST /chat вызван");
                var body = await ReadBody(context);

                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                await LoggingService.LogAsync($"[ChatService] Десериализован чат: {chatname}, токен: {_aes.Decrypt(token)}");
                Chat chat = new(_aes.Decrypt(token), chatname);

                return Results.Ok(new { message = _applicationContext.AddChat(chat)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }

        });
        app.MapPost("/join", async (HttpContext context) =>
        {
            try
            {
                await LoggingService.LogAsync("[ChatService] POST /join вызван");
                var body = await ReadBody(context);

                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                Chat chat;
                if (int.TryParse(chatname, out _))
                    chat = _applicationContext.GetChat(int.Parse(chatname));
                else
                    chat = _applicationContext.GetChat(chatname);

                await LoggingService.LogAsync($"[ChatService] Десериализован чат: {chatname}, токен: {_aes.Decrypt(token)}");

                return Results.Ok(new { message = _applicationContext.JoinChat(chat, _aes.Decrypt(token))});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapPost("/transfer", async (HttpContext context) =>
        {
            try
            {
                await LoggingService.LogAsync("[ChatService] POST /transfer вызван");
                var body = await ReadBody(context);

                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                var newOwner = JsonNode.Parse(body)["newOwner"]!.GetValue<string>();
                Chat chat;
                if (int.TryParse(chatname, out _))
                    chat = _applicationContext.GetChat(int.Parse(chatname));
                else
                    chat = _applicationContext.GetChat(chatname);

                await LoggingService.LogAsync($"[ChatService] Десериализован чат: {chatname}, токен: {_aes.Decrypt(token)}, новый владелец: {newOwner}");

                return Results.Ok(new { message = _applicationContext.TransferOwnership(chat, _aes.Decrypt(token), newOwner)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapPost("/leave", async (HttpContext context) =>
        {
            try
            {
                await LoggingService.LogAsync("[ChatService] POST /leave вызван");
                var body = await ReadBody(context);

                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                Chat chat;
                if (int.TryParse(chatname, out _))
                    chat = _applicationContext.GetChat(int.Parse(chatname));
                else
                    chat = _applicationContext.GetChat(chatname);

                await LoggingService.LogAsync($"[ChatService] Десериализован чат: {chatname}, токен: {_aes.Decrypt(token)}");

                return Results.Ok(new { message = _applicationContext.LeaveChat(chat, _aes.Decrypt(token))});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapPost("/message", async (HttpContext context) =>
        {
            try
            {
                await LoggingService.LogAsync("[ChatService] POST /message вызван");
                var body = await ReadBody(context);

                var chatname = JsonNode.Parse(body)["chat"]!.GetValue<string>();
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                var message = JsonNode.Parse(body)["message"]!.GetValue<string>();
                Chat chat;
                if (int.TryParse(chatname, out _))
                    chat = _applicationContext.GetChat(int.Parse(chatname));
                else
                    chat = _applicationContext.GetChat(chatname);
                message = _rsa.Decrypt(message);
                await LoggingService.LogAsync($"[ChatService] Десериализован чат: {chatname}, токен: {_aes.Decrypt(token)}, сообщение: {message}");

                return Results.Ok(new { message = _applicationContext.SendMessage(chat, _aes.Decrypt(token), message)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapGet("/pubkey", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] GET /pubkey вызван");

            return Results.Ok(new { message = Convert.ToBase64String(RSAClass.GetPublicKey())});
        });
    }
}