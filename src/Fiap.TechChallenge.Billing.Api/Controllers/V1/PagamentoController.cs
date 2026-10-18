using System.Text;
using Fiap.TechChallenge.Billing.Application.DTOs.Pagamento;
using Fiap.TechChallenge.Billing.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.Billing.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v1/pagamentos")]
    [Produces("application/json")]
    public class PagamentoController : ControllerBase
    {
        private readonly IPagamentoAppService _pagamentoAppService;
        private readonly ILogger<PagamentoController> _logger;

        public PagamentoController(IPagamentoAppService pagamentoAppService, ILogger<PagamentoController> logger)
        {
            _pagamentoAppService = pagamentoAppService ?? throw new ArgumentNullException(nameof(pagamentoAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inicia uma cobrança PIX gerando QR Code dinâmico via Mercado Pago.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IniciarCobrancaPix([FromBody] CriarPagamentoDto dto, CancellationToken cancellationToken)
        {
            var pagamento = await _pagamentoAppService.IniciarCobrancaPixAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = pagamento.Id }, pagamento);
        }

        /// <summary>
        /// Consulta status e detalhes do pagamento pelo ID interno.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var pagamento = await _pagamentoAppService.ObterPorIdAsync(id, cancellationToken);
            if (pagamento == null)
                return NotFound(new { error = $"Pagamento com ID '{id}' não encontrado." });

            return Ok(pagamento);
        }

        /// <summary>
        /// Consulta status de pagamento vinculado a uma Ordem de Serviço específica.
        /// </summary>
        [HttpGet("os/{osId:guid}")]
        [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorOrdemServicoId(Guid osId, CancellationToken cancellationToken)
        {
            var pagamento = await _pagamentoAppService.ObterPorOrdemServicoIdAsync(osId, cancellationToken);
            if (pagamento == null)
                return NotFound(new { error = $"Nenhum pagamento localizado para a Ordem de Serviço '{osId}'." });

            return Ok(pagamento);
        }

        /// <summary>
        /// Endpoint público para recebimento de notificações Webhook (IPN) do Mercado Pago.
        /// </summary>
        [HttpPost("webhook/mercadopago")]
        [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> WebhookMercadoPago(CancellationToken cancellationToken)
        {
            string? xSignature = Request.Headers["x-signature"].FirstOrDefault();
            string? xRequestId = Request.Headers["x-request-id"].FirstOrDefault();

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string rawBody = await reader.ReadToEndAsync(cancellationToken);

            _logger.LogInformation("Webhook recebido do Mercado Pago. RequestId: {RequestId}, SignaturePresente: {HasSignature}",
                xRequestId, !string.IsNullOrWhiteSpace(xSignature));

            var resultado = await _pagamentoAppService.ProcessarWebhookMercadoPagoAsync(
                xSignature,
                xRequestId,
                rawBody,
                cancellationToken);

            return Ok(resultado);
        }

        /// <summary>
        /// Executa estorno do pagamento (ação compensatória da Saga Pattern).
        /// </summary>
        [HttpPost("{id:guid}/estorno")]
        [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EstornarPagamento(Guid id, [FromBody] EstornoPagamentoDto dto, CancellationToken cancellationToken)
        {
            var resultado = await _pagamentoAppService.EstornarAsync(id, dto, cancellationToken);
            return Ok(resultado);
        }
    }
}
