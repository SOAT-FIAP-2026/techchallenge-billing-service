using Fiap.TechChallenge.Billing.Application.Configurations;
using Fiap.TechChallenge.Billing.External.Configurations;
using Fiap.TechChallenge.Billing.Infrastructure.Configurations;

namespace Fiap.TechChallenge.Billing.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddInfrastructure(configuration);
            services.AddApplication();
            services.AddExternal(configuration);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
    }
}
