using System.Text.Json;
using DbsClasses;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;

namespace Microservices;


public sealed class UserService : IWebMicroservice
{
    private readonly AESClass _aes = new();
    public async Task<string> ReadBody(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        return body;
    }
    private readonly UserDb _applicationContext = new();
    public string Name => "UserService";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[UserService] Веб запущен.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggingService.LogAsync("[UserService] Веб остановлен.");
        return Task.CompletedTask;
    }

    public Task UpdateAsync(float deltaTime)
    {
        return Task.CompletedTask;
    }

    public void MapEndpoints(WebApplication app)
    {
        app.MapPost("/register", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] POST /register вызван");

            var body = await ReadBody(context);
            User user = new();
            try
            {
                user.Name = JsonNode.Parse(body)["user"]!.GetValue<string>();
                user.Password = JsonNode.Parse(body)["password"]!.GetValue<string>();
                await LoggingService.LogAsync($"[UserService] Десериализован пользователь: {user.Name}");
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
            user.Password = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.Password)));
            if (user != null && user.Password != null)
                return Results.Ok(new { message = _applicationContext.AddUser(user)});
            else
                return Results.BadRequest(new { message = "User registration failed"});
        });
        app.MapPost("/login", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] POST /login вызван");

            var body = await ReadBody(context);
            User user;
            try
            {
                var nullableUser = JsonSerializer.Deserialize<User>(body);
                if (nullableUser == null)
                    return Results.BadRequest(new { message = "User login failed"});
                user = nullableUser;
                await LoggingService.LogAsync($"[UserService] Десериализован пользователь: {user.Name}");
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(ex.Message);
            }
            user.Password = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.Password)));
            var token = _applicationContext.LoginUser(user);
            await LoggingService.LogAsync($"{user.Name} logged in");
            if (user != null && user.Password != null && token.Item2 != null)
                return Results.Ok(new { message = token.Item1, token = token.Item2});
            else
                return Results.BadRequest(new { message = "User login failed"});
        });
        app.MapPost("/token", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] POST /token вызван");

            var body = await ReadBody(context);
            if (body.Length == 0)
                return Results.BadRequest(new { message = "Token is empty"});
            try
            {
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                return Results.Ok(new { message = _aes.Decrypt(token)});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(new { message = ex.Message});
            }
        });
        app.MapGet("/users", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] GET /users вызван");

            return Results.Ok(new { message = _applicationContext.GetUsers()});
        });
        app.MapPost("/info", async (HttpContext context) =>
        {
            await LoggingService.LogAsync("[UserService] GET /info вызван");
            var body = await ReadBody(context);
            if (body.Length == 0)
                return Results.BadRequest(new { message = "Token is empty"});
            try
            {
                var token = JsonNode.Parse(body)["token"]!.GetValue<string>();
                return Results.Ok(new { message = _applicationContext.GetUser(_aes.Decrypt(token))});
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync(ex.Message);
                return Results.BadRequest(new { message = ex.Message});
            }    
        });
    }
}