using Fiap.TechChallenge.Billing.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fiap.TechChallenge.Billing.Api.Observability
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly IMongoDbContext _dbContext;

        public MongoDbHealthCheck(IMongoDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var isAlive = await _dbContext.PingAsync(cancellationToken);
                if (isAlive)
                {
                    return HealthCheckResult.Healthy("Conexão com MongoDB ativa e responsiva.");
                }

                return HealthCheckResult.Unhealthy("MongoDB não respondeu ao comando ping.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Falha ao comunicar com MongoDB.", ex);
            }
        }
    }
}
