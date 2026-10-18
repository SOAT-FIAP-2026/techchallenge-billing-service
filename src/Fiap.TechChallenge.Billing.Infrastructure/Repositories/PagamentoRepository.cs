using Fiap.TechChallenge.Billing.Domain.Entities;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Repository;
using Fiap.TechChallenge.Billing.Infrastructure.Data;
using MongoDB.Driver;

namespace Fiap.TechChallenge.Billing.Infrastructure.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly IMongoDbContext _context;

        public PagamentoRepository(IMongoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Pagamentos
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Pagamento?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default)
        {
            return await _context.Pagamentos
                .Find(p => p.IdOrdemServico == idOrdemServico)
                .SortByDescending(p => p.DataCriacao)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Pagamento?> ObterPorExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
                return null;

            return await _context.Pagamentos
                .Find(p => p.ExternalPaymentId == externalPaymentId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InserirAsync(Pagamento pagamento, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pagamento);
            await _context.Pagamentos.InsertOneAsync(pagamento, cancellationToken: cancellationToken);
        }

        public async Task AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pagamento);
            var filter = Builders<Pagamento>.Filter.Eq(p => p.Id, pagamento.Id);
            await _context.Pagamentos.ReplaceOneAsync(filter, pagamento, cancellationToken: cancellationToken);
        }
    }
}
