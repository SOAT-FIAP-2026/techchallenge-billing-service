using Fiap.TechChallenge.Billing.Application.DTOs.Orcamento;
using Fiap.TechChallenge.Billing.Application.Interfaces;
using Fiap.TechChallenge.Billing.Domain.Entities;
using Fiap.TechChallenge.Billing.Domain.Exceptions;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.Billing.Application.Services
{
    public class OrcamentoAppService : IOrcamentoAppService
    {
        private readonly IOrcamentoRepository _orcamentoRepository;
        private readonly ILogger<OrcamentoAppService> _logger;

        public OrcamentoAppService(IOrcamentoRepository orcamentoRepository, ILogger<OrcamentoAppService> logger)
        {
            _orcamentoRepository = orcamentoRepository ?? throw new ArgumentNullException(nameof(orcamentoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrcamentoResponseDto> CriarAsync(CriarOrcamentoDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            _logger.LogInformation("Criando novo orçamento para OS '{OrdemServicoId}'.", dto.IdOrdemServico);

            var itens = dto.Itens.Select(i => new ItemOrcamento(
                i.Descricao,
                i.Tipo,
                i.Quantidade,
                i.ValorUnitario,
                i.ItemCatalogoId)).ToList();

            var validade = dto.ValidadeDias.HasValue
                ? DateTime.UtcNow.AddDays(dto.ValidadeDias.Value)
                : DateTime.UtcNow.AddDays(7);

            var orcamento = new Orcamento(dto.IdOrdemServico, itens, dto.Observacoes, validade);
            await _orcamentoRepository.InserirAsync(orcamento, cancellationToken);

            _logger.LogInformation("Orçamento '{OrcamentoId}' criado com sucesso para OS '{OrdemServicoId}' no valor total de {ValorTotal:C}.",
                orcamento.Id, orcamento.IdOrdemServico, orcamento.ValorTotal);

            return MapearParaDto(orcamento);
        }

        public async Task<OrcamentoResponseDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var orcamento = await _orcamentoRepository.ObterPorIdAsync(id, cancellationToken);
            return orcamento == null ? null : MapearParaDto(orcamento);
        }

        public async Task<OrcamentoResponseDto?> ObterPorOrdemServicoIdAsync(Guid idOrdemServico, CancellationToken cancellationToken = default)
        {
            var orcamento = await _orcamentoRepository.ObterPorOrdemServicoIdAsync(idOrdemServico, cancellationToken);
            return orcamento == null ? null : MapearParaDto(orcamento);
        }

        public async Task<OrcamentoResponseDto> AprovarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var orcamento = await _orcamentoRepository.ObterPorIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Orcamento), id);

            orcamento.Aprovar();
            await _orcamentoRepository.AtualizarAsync(orcamento, cancellationToken);

            _logger.LogInformation("Orçamento '{OrcamentoId}' aprovado com sucesso.", id);
            return MapearParaDto(orcamento);
        }

        public async Task<OrcamentoResponseDto> RejeitarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default)
        {
            var orcamento = await _orcamentoRepository.ObterPorIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Orcamento), id);

            orcamento.Rejeitar(motivo);
            await _orcamentoRepository.AtualizarAsync(orcamento, cancellationToken);

            _logger.LogInformation("Orçamento '{OrcamentoId}' rejeitado.", id);
            return MapearParaDto(orcamento);
        }

        public async Task<OrcamentoResponseDto> CancelarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default)
        {
            var orcamento = await _orcamentoRepository.ObterPorIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Orcamento), id);

            orcamento.Cancelar(motivo);
            await _orcamentoRepository.AtualizarAsync(orcamento, cancellationToken);

            _logger.LogInformation("Orçamento '{OrcamentoId}' cancelado.", id);
            return MapearParaDto(orcamento);
        }

        private static OrcamentoResponseDto MapearParaDto(Orcamento orcamento)
        {
            return new OrcamentoResponseDto
            {
                Id = orcamento.Id,
                IdOrdemServico = orcamento.IdOrdemServico,
                ValorTotal = orcamento.ValorTotal,
                Status = orcamento.Status.ToString(),
                Observacoes = orcamento.Observacoes,
                DataCriacao = orcamento.DataCriacao,
                DataAprovacao = orcamento.DataAprovacao,
                DataExpiracao = orcamento.DataExpiracao,
                Itens = orcamento.Itens.Select(i => new ItemOrcamentoDto
                {
                    Id = i.Id,
                    ItemCatalogoId = i.ItemCatalogoId,
                    Descricao = i.Descricao,
                    Tipo = i.Tipo,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario
                }).ToList()
            };
        }
    }
}
