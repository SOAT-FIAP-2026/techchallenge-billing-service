using Fiap.TechChallenge.Billing.Api.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Fiap.TechChallenge.Billing.Api.Configurations
{
    public static class ObservabilityConfig
    {
        public static void AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<BillingObservabilityMetrics>();

            services.AddHealthChecks()
                .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["ready"]);

            string serviceName = configuration["OTEL_SERVICE_NAME"]
                ?? configuration["Observability:ServiceName"]
                ?? "techchallenge-billing-service";
            string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString(),
                        serviceInstanceId: Environment.MachineName)
                    .AddAttributes([
                        new KeyValuePair<string, object>("service.namespace", "techchallenge")
                    ]))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        tracing.AddOtlpExporter();
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddMeter(BillingObservabilityMetrics.MeterName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddPrometheusExporter();

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        metrics.AddOtlpExporter();
                });
        }
    }
}
