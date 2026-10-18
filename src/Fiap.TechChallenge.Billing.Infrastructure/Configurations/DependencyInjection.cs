using Fiap.TechChallenge.Billing.Domain.Interfaces.Repository;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Fiap.TechChallenge.Billing.Infrastructure.Aws;
using Fiap.TechChallenge.Billing.Infrastructure.Data;
using Fiap.TechChallenge.Billing.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.Billing.Infrastructure.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configurações MongoDB
            services.Configure<MongoDbSettings>(options =>
            {
                configuration.GetSection(MongoDbSettings.SectionName).Bind(options);

                // Permite sobrescrita por ConnectionStrings:MongoDb ou MONGODB_CONNECTION_STRING
                var connStr = configuration.GetConnectionString("MongoDb")
                    ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");

                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    options.ConnectionString = connStr;
                }
            });

            services.AddSingleton<IMongoDbContext, MongoDbContext>();
            services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
            services.AddScoped<IPagamentoRepository, PagamentoRepository>();

            // 2. Configurações AWS SSM Parameter Store
            services.Configure<AwsSsmSettings>(options =>
            {
                configuration.GetSection(AwsSsmSettings.SectionName).Bind(options);

                var ssmEnabledEnv = Environment.GetEnvironmentVariable("AWS_SSM_ENABLED");
                if (bool.TryParse(ssmEnabledEnv, out var ssmEnabled))
                {
                    options.Enabled = ssmEnabled;
                }

                var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");
                if (!string.IsNullOrWhiteSpace(awsRegion))
                {
                    options.Region = awsRegion;
                }
            });

            services.AddSingleton<ISsmSecretResolver, AwsSsmSecretResolver>();

            return services;
        }
    }
}
