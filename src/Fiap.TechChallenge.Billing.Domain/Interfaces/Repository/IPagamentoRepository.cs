using Fiap.TechChallenge.Billing.Domain.Entities;

namespace Fiap.TechChallenge.Billing.Domain.Interfaces.Repository
{
    public interface IPagamentoRepository
    {
        Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Pagamento?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default);
        Task<Pagamento?> ObterPorExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);
        Task InserirAsync(Pagamento pagamento, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken = default);
    }
}
