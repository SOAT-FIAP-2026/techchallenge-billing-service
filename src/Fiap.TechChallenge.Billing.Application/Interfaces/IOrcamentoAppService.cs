using Fiap.TechChallenge.Billing.Application.DTOs.Orcamento;

namespace Fiap.TechChallenge.Billing.Application.Interfaces
{
    public interface IOrcamentoAppService
    {
        Task<OrcamentoResponseDto> CriarAsync(CriarOrcamentoDto dto, CancellationToken cancellationToken = default);
        Task<OrcamentoResponseDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrcamentoResponseDto?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default);
        Task<OrcamentoResponseDto> AprovarAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrcamentoResponseDto> RejeitarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default);
        Task<OrcamentoResponseDto> CancelarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default);
    }
}
