using System.Text.Json;
using Fiap.TechChallenge.Billing.Application.DTOs.Pagamento;
using Fiap.TechChallenge.Billing.Application.Interfaces;
using Fiap.TechChallenge.Billing.Domain.Entities;
using Fiap.TechChallenge.Billing.Domain.Enums;
using Fiap.TechChallenge.Billing.Domain.Exceptions;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Repository;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.Billing.Application.Services
{
    public class PagamentoAppService : IPagamentoAppService
    {
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly IOrcamentoRepository _orcamentoRepository;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly ILogger<PagamentoAppService> _logger;

        public PagamentoAppService(
            IPagamentoRepository pagamentoRepository,
            IOrcamentoRepository orcamentoRepository,
            IMercadoPagoService mercadoPagoService,
            ILogger<PagamentoAppService> logger)
        {
            _pagamentoRepository = pagamentoRepository ?? throw new ArgumentNullException(nameof(pagamentoRepository));
            _orcamentoRepository = orcamentoRepository ?? throw new ArgumentNullException(nameof(orcamentoRepository));
            _mercadoPagoService = mercadoPagoService ?? throw new ArgumentNullException(nameof(mercadoPagoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagamentoResponseDto> IniciarCobrancaPixAsync(CriarPagamentoDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            _logger.LogInformation("Iniciando cobrança PIX para OS '{OrdemServicoId}', Orçamento '{OrcamentoId}', Valor: {Valor:C}.",
                dto.IdOrdemServico, dto.IdOrcamento, dto.Valor);

            var orcamento = await _orcamentoRepository.ObterPorIdAsync(dto.IdOrcamento, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Orcamento), dto.IdOrcamento);

            var dadosPagador = new DadosPagador(dto.Pagador.Cpf, dto.Pagador.Nome, dto.Pagador.Email);
            var pagamento = new Pagamento(dto.IdOrdemServico, dto.IdOrcamento, dto.Valor, dadosPagador, MetodoPagamento.Pix);

            // Chamada à API Mercado Pago
            var cobrancaRequest = new CobrancaPixRequest(
                ExternalReference: dto.IdOrdemServico.ToString(),
                Descricao: $"Ordem de Serviço #{dto.IdOrdemServico}",
                Valor: dto.Valor,
                Cpf: dadosPagador.Cpf,
                Nome: dadosPagador.Nome,
                Email: dadosPagador.Email
            );

            var resultadoPix = await _mercadoPagoService.GerarCobrancaPixAsync(cobrancaRequest, cancellationToken);
            pagamento.RegistrarCobrancaGerada(resultadoPix.ExternalPaymentId, resultadoPix.QrCode ?? string.Empty, resultadoPix.QrCodeBase64 ?? string.Empty);

            await _pagamentoRepository.InserirAsync(pagamento, cancellationToken);

            _logger.LogInformation("Pagamento '{PagamentoId}' persistido com ID Mercado Pago '{ExternalPaymentId}'.",
                pagamento.Id, pagamento.ExternalPaymentId);

            return MapearParaDto(pagamento);
        }

        public async Task<PagamentoResponseDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var pagamento = await _pagamentoRepository.ObterPorIdAsync(id, cancellationToken);
            return pagamento == null ? null : MapearParaDto(pagamento);
        }

        public async Task<PagamentoResponseDto?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default)
        {
            var pagamento = await _pagamentoRepository.ObterPorOrdemServicoIdAsync(idOrdemServico, cancellationToken);
            return pagamento == null ? null : MapearParaDto(pagamento);
        }

        public async Task<PagamentoResponseDto> ProcessarWebhookMercadoPagoAsync(
            string? xSignature,
            string? xRequestId,
            string rawBody,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processando notificação de Webhook do Mercado Pago.");

            // 1. Validação Criptográfica HMAC SHA-256
            var assinaturaValida = _mercadoPagoService.ValidarAssinaturaWebhook(xSignature, xRequestId, rawBody);
            if (!assinaturaValida)
            {
                _logger.LogWarning("Assinatura do webhook rejeitada. Rejeitando requisição.");
                throw new DomainException("Assinatura de Webhook inválida.");
            }

            // 2. Extração de metadados do Webhook
            string? externalPaymentId = null;
            string? action = null;

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("action", out var act))
                    action = act.GetString();

                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var idProp))
                    externalPaymentId = idProp.GetString();
                else if (root.TryGetProperty("id", out var idDirect))
                    externalPaymentId = idDirect.GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao deserializar corpo do Webhook: {RawBody}", rawBody);
                throw new DomainException("Payload de webhook em formato inválido.");
            }

            if (string.IsNullOrWhiteSpace(externalPaymentId))
            {
                _logger.LogWarning("Identificador de pagamento não localizado no corpo do Webhook.");
                throw new DomainException("Identificador de pagamento ausente no Webhook.");
            }

            // 3. Consulta à API Mercado Pago para validar status oficial
            var statusOficial = await _mercadoPagoService.ConsultarPagamentoAsync(externalPaymentId, cancellationToken);

            // 4. Localizar o Pagamento correspondente no banco
            var pagamento = await _pagamentoRepository.ObterPorExternalIdAsync(externalPaymentId, cancellationToken);

            if (pagamento == null && !string.IsNullOrWhiteSpace(statusOficial.ExternalReference) &&
                Guid.TryParse(statusOficial.ExternalReference, out var osId))
            {
                pagamento = await _pagamentoRepository.ObterPorOrdemServicoIdAsync(osId, cancellationToken);
            }

            if (pagamento == null)
            {
                _logger.LogWarning("Nenhum pagamento correspondente foi localizado para o ID externo '{ExternalPaymentId}'.", externalPaymentId);
                throw new EntityNotFoundException(nameof(Pagamento), externalPaymentId);
            }

            // 5. Registro do Webhook para histórico e auditoria
            var registro = new RegistroWebhook(
                externalEventId: xRequestId ?? externalPaymentId,
                action: action ?? statusOficial.Status,
                statusRecebido: statusOficial.Status,
                assinaturaValida: assinaturaValida,
                payloadJson: rawBody,
                mensagemProcessamento: $"Status: {statusOficial.Status}, Detalhe: {statusOficial.StatusDetail}"
            );
            pagamento.AdicionarRegistroWebhook(registro);

            // 6. Atualização de Status com Idempotência
            if (statusOficial.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
            {
                pagamento.ConfirmarPagamento(externalPaymentId);
                _logger.LogInformation("Pagamento '{PagamentoId}' confirmado com sucesso via Webhook.", pagamento.Id);
            }
            else if (statusOficial.Status.Equals("rejected", StringComparison.OrdinalIgnoreCase) ||
                     statusOficial.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
            {
                pagamento.FalharPagamento($"Transação rejeitada pelo Mercado Pago: {statusOficial.StatusDetail}");
                _logger.LogInformation("Pagamento '{PagamentoId}' marcado como falhado via Webhook. Motivo: {Detail}",
                    pagamento.Id, statusOficial.StatusDetail);
            }

            await _pagamentoRepository.AtualizarAsync(pagamento, cancellationToken);
            return MapearParaDto(pagamento);
        }

        public async Task<PagamentoResponseDto> EstornarAsync(Guid id, EstornoPagamentoDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var pagamento = await _pagamentoRepository.ObterPorIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Pagamento), id);

            if (string.IsNullOrWhiteSpace(pagamento.ExternalPaymentId))
                throw new DomainException("Pagamento não possui ID externo registrado no gateway para estorno.");

            var resultado = await _mercadoPagoService.RealizarEstornoAsync(pagamento.ExternalPaymentId, cancellationToken);
            if (!resultado.Sucesso)
            {
                throw new PaymentProcessingException($"Falha ao estornar pagamento no Mercado Pago: {resultado.Mensagem}");
            }

            pagamento.Estornar(dto.Motivo);
            await _pagamentoRepository.AtualizarAsync(pagamento, cancellationToken);

            _logger.LogInformation("Pagamento '{PagamentoId}' estornado com sucesso. Motivo: {Motivo}", id, dto.Motivo);
            return MapearParaDto(pagamento);
        }

        private static PagamentoResponseDto MapearParaDto(Pagamento pagamento)
        {
            return new PagamentoResponseDto
            {
                Id = pagamento.Id,
                IdOrdemServico = pagamento.IdOrdemServico,
                IdOrcamento = pagamento.IdOrcamento,
                ExternalPaymentId = pagamento.ExternalPaymentId,
                Valor = pagamento.Valor,
                Status = pagamento.Status.ToString(),
                Metodo = pagamento.Metodo.ToString(),
                QrCode = pagamento.QrCode,
                QrCodeBase64 = pagamento.QrCodeBase64,
                DataCriacao = pagamento.DataCriacao,
                DataConfirmacao = pagamento.DataConfirmacao,
                MotivoFalha = pagamento.MotivoFalha,
                Pagador = new DadosPagadorDto
                {
                    Cpf = pagamento.Pagador.Cpf,
                    Nome = pagamento.Pagador.Nome,
                    Email = pagamento.Pagador.Email
                }
            };
        }
    }
}
