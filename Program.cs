using Microsoft.Extensions.AI;
using StackAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5151);
    options.ListenLocalhost(7151, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var ollamaUrl = builder.Configuration["Ollama:Url"]!;
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "qwen2.5:3b";

builder.Services.AddChatClient(new OllamaChatClient(
    new Uri(ollamaUrl),
    ollamaModel),
    lifetime: ServiceLifetime.Singleton);

builder.Services.AddScoped<IDomExtractionService, DomExtractionService>();
builder.Services.AddScoped<IStackAnalysisService, StackAnalysisService>();
builder.Services.AddScoped<IStackAnalyzerOrchestrationService, StackAnalyzerOrchestrationService>();

builder.Services.AddHttpClient();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();