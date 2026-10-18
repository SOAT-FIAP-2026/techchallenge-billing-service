using Fiap.TechChallenge.Billing.Api;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Fiap.TechChallenge.Billing.Infrastructure.Data;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.UseUtcTimestamp = true;
});

if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    builder.Logging.AddOpenTelemetry(options =>
    {
        options.IncludeScopes = true;
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        options.AddOtlpExporter();
    });
}

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Inicialização de coleções e índices no MongoDB
using (var scope = app.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    try
    {
        await mongoContext.EnsureIndexesAsync(cts.Token);
    }
    catch (Exception ex)
    {
        var initLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        initLogger.LogWarning(ex, "MongoDB offline ou não responsivo na inicialização. A API iniciará e responderá às requisições normalmente.");
    }

    // Verificação de resolução de parâmetros no AWS SSM se habilitado
    var ssmResolver = scope.ServiceProvider.GetRequiredService<ISsmSecretResolver>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Serviço de Billing e Pagamento inicializado com sucesso.");
}

startup.Configure(app, app.Environment);

app.Run();

public partial class Program { }
