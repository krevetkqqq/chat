using DbsClasses;
using Microservices;

var builder = WebApplication.CreateBuilder(args);

// не знаю зачем нужен пиздец 2 раза, но оно работает
#region пиздец
builder.Services.AddSingleton<IMicroservice, AppService>();
builder.Services.AddSingleton<IMicroservice, HealthService>();
builder.Services.AddSingleton<IMicroservice, ChatService>();
builder.Services.AddSingleton<IMicroservice, UserService>();

builder.Services.AddSingleton<AppService>();
builder.Services.AddSingleton<HealthService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<UserService>();

builder.Services.AddSingleton<MicroserviceHost>();

builder.Logging.ClearProviders(); 
builder.Logging.SetMinimumLevel(LogLevel.None);

new RSAClass();
new AESClass();
#endregion

var app = builder.Build();

foreach (var webService in app.Services.GetServices<IMicroservice>().OfType<IWebMicroservice>())
{
    webService.MapEndpoints(app);
}

var host = app.Services.GetRequiredService<MicroserviceHost>();
var cts = new CancellationTokenSource();

await host.StartAllAsync();

var updateTask = host.RunUpdateLoopAsync(intervalMs: 1000, cts.Token);

app.Run();

cts.Cancel();
await updateTask;
await host.StopAllAsync();