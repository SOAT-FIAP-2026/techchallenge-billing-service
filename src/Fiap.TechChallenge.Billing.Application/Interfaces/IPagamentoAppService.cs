using Fiap.TechChallenge.Billing.Application.DTOs.Pagamento;

namespace Fiap.TechChallenge.Billing.Application.Interfaces
{
    public interface IPagamentoAppService
    {
        Task<PagamentoResponseDto> IniciarCobrancaPixAsync(CriarPagamentoDto dto, CancellationToken cancellationToken = default);
        Task<PagamentoResponseDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagamentoResponseDto?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default);
        Task<PagamentoResponseDto> ProcessarWebhookMercadoPagoAsync(string? xSignature, string? xRequestId, string rawBody, CancellationToken cancellationToken = default);
        Task<PagamentoResponseDto> EstornarAsync(Guid id, EstornoPagamentoDto dto, CancellationToken cancellationToken = default);
    }
}
