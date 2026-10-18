using System.Diagnostics;
using System.Text.Json;
using Fiap.TechChallenge.Billing.Domain.Exceptions;

namespace Fiap.TechChallenge.Billing.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                string correlationId = context.Request.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault()
                    ?? context.TraceIdentifier;
                string traceId = Activity.Current?.TraceId.ToHexString() ?? context.TraceIdentifier;
                int statusCode = GetStatusCode(ex);

                _logger.LogError(
                    ex,
                    "Exceção capturada no processamento de {Method} {Path}. Código HTTP: {StatusCode}.",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode);

                context.Response.Clear();
                context.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var payload = new ErrorResponse(
                    statusCode,
                    GetMessage(ex),
                    traceId,
                    correlationId
                );

                var json = JsonSerializer.Serialize(payload, JsonOptions);
                await context.Response.WriteAsync(json);
            }
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                PaymentProcessingException => StatusCodes.Status502BadGateway,
                DomainException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetMessage(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException => exception.Message,
                PaymentProcessingException => exception.Message,
                DomainException => exception.Message,
                ArgumentException => exception.Message,
                InvalidOperationException => exception.Message,
                UnauthorizedAccessException => exception.Message,
                _ => "Ocorreu um erro interno inesperado no serviço de billing."
            };
        }

        private sealed record ErrorResponse(
            int StatusCode,
            string Message,
            string TraceId,
            string CorrelationId
        );
    }
}
