using Fiap.TechChallenge.Billing.Domain.Entities;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Repository;
using Fiap.TechChallenge.Billing.Infrastructure.Data;
using MongoDB.Driver;

namespace Fiap.TechChallenge.Billing.Infrastructure.Repositories
{
    public class OrcamentoRepository : IOrcamentoRepository
    {
        private readonly IMongoDbContext _context;

        public OrcamentoRepository(IMongoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Orcamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orcamentos
                .Find(o => o.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Orcamento?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default)
        {
            return await _context.Orcamentos
                .Find(o => o.IdOrdemServico == idOrdemServico)
                .SortByDescending(o => o.DataCriacao)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InserirAsync(Orcamento orcamento, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(orcamento);
            await _context.Orcamentos.InsertOneAsync(orcamento, cancellationToken: cancellationToken);
        }

        public async Task AtualizarAsync(Orcamento orcamento, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(orcamento);
            var filter = Builders<Orcamento>.Filter.Eq(o => o.Id, orcamento.Id);
            await _context.Orcamentos.ReplaceOneAsync(filter, orcamento, cancellationToken: cancellationToken);
        }
    }
}
