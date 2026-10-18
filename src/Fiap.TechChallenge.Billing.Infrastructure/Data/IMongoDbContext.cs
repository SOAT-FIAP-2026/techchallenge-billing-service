using Fiap.TechChallenge.Billing.Domain.Entities;
using MongoDB.Driver;

namespace Fiap.TechChallenge.Billing.Infrastructure.Data
{
    public interface IMongoDbContext
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
        IMongoCollection<Orcamento> Orcamentos { get; }
        IMongoCollection<Pagamento> Pagamentos { get; }
        Task<bool> PingAsync(CancellationToken cancellationToken = default);
        Task EnsureIndexesAsync(CancellationToken cancellationToken = default);
    }
}
