using System.Diagnostics;
using Fiap.TechChallenge.Billing.Api.Observability;

namespace Fiap.TechChallenge.Billing.Api.Middlewares
{
    public sealed class RequestObservabilityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BillingObservabilityMetrics _metrics;
        private readonly ILogger<RequestObservabilityMiddleware> _logger;

        public RequestObservabilityMiddleware(
            RequestDelegate next,
            BillingObservabilityMetrics metrics,
            ILogger<RequestObservabilityMiddleware> logger)
        {
            _next = next;
            _metrics = metrics;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            long startedAt = Stopwatch.GetTimestamp();

            try
            {
                await _next(context);
            }
            finally
            {
                TimeSpan duration = Stopwatch.GetElapsedTime(startedAt);
                int statusCode = context.Response.StatusCode;
                _metrics.RecordHttpRequest(duration, context.Request.Method, statusCode);

                if (statusCode >= StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(
                        "Falha HTTP no serviço de Billing: Método {Method}, Caminho {Path}, Status {StatusCode}, Duração {DurationMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        duration.TotalMilliseconds);
                }
            }
        }
    }
}
