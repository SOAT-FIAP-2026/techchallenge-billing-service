using Fiap.TechChallenge.Billing.Application.Interfaces;
using Fiap.TechChallenge.Billing.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.Billing.Application.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrcamentoAppService, OrcamentoAppService>();
            services.AddScoped<IPagamentoAppService, PagamentoAppService>();

            return services;
        }
    }
}
