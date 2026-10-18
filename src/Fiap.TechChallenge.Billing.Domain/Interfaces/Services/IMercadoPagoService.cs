namespace Fiap.TechChallenge.Billing.Domain.Interfaces.Services
{
    public record CobrancaPixRequest(
        string ExternalReference,
        string Descricao,
        decimal Valor,
        string Cpf,
        string Nome,
        string Email,
        string? NotificationUrl = null
    );

    public record CobrancaPixResult(
        string ExternalPaymentId,
        string Status,
        string? QrCode,
        string? QrCodeBase64,
        string? TicketUrl,
        string RawResponse
    );

    public record ConsultaPagamentoResult(
        string ExternalPaymentId,
        string Status,
        string? StatusDetail,
        decimal Valor,
        string? ExternalReference,
        string RawResponse
    );

    public record EstornoResult(
        bool Sucesso,
        string ExternalPaymentId,
        string Status,
        string? Mensagem
    );

    public interface IMercadoPagoService
    {
        Task<CobrancaPixResult> GerarCobrancaPixAsync(CobrancaPixRequest request, CancellationToken cancellationToken = default);
        Task<ConsultaPagamentoResult> ConsultarPagamentoAsync(string externalPaymentId, CancellationToken cancellationToken = default);
        Task<EstornoResult> RealizarEstornoAsync(string externalPaymentId, CancellationToken cancellationToken = default);
        bool ValidarAssinaturaWebhook(string? xSignatureHeader, string? xRequestId, string requestBody);
    }
}
