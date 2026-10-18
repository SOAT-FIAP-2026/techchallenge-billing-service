using Fiap.TechChallenge.Billing.Domain.Entities;

namespace Fiap.TechChallenge.Billing.Domain.Interfaces.Repository
{
    public interface IOrcamentoRepository
    {
        Task<Orcamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Orcamento?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default);
        Task InserirAsync(Orcamento orcamento, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Orcamento orcamento, CancellationToken cancellationToken = default);
    }
}
