using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Fiap.TechChallenge.Billing.External.MercadoPago;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Fiap.TechChallenge.Billing.External.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternal(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MercadoPagoSettings>(options =>
            {
                configuration.GetSection(MercadoPagoSettings.SectionName).Bind(options);

                var accessToken = Environment.GetEnvironmentVariable("MERCADOPAGO_ACCESS_TOKEN");
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    options.AccessToken = accessToken;
                }

                var webhookSecret = Environment.GetEnvironmentVariable("MERCADOPAGO_WEBHOOK_SECRET");
                if (!string.IsNullOrWhiteSpace(webhookSecret))
                {
                    options.WebhookSecret = webhookSecret;
                }
            });

            // Configuração do HttpClient com política de retry Polly para resiliência
            services.AddHttpClient<IMercadoPagoService, MercadoPagoClient>()
                .AddPolicyHandler(GetRetryPolicy());

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
