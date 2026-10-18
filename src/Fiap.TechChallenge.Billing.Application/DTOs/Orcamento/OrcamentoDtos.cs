using System.ComponentModel.DataAnnotations;
using Fiap.TechChallenge.Billing.Domain.Enums;

namespace Fiap.TechChallenge.Billing.Application.DTOs.Orcamento
{
    public class ItemOrcamentoDto
    {
        public Guid? Id { get; set; }
        public Guid? ItemCatalogoId { get; set; }

        [Required(ErrorMessage = "A descrição do item é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        public TipoItemOrcamento Tipo { get; set; } = TipoItemOrcamento.Servico;

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de no mínimo 1.")]
        public int Quantidade { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero.")]
        public decimal ValorUnitario { get; set; }

        public decimal ValorTotal => Quantidade * ValorUnitario;
    }

    public class CriarOrcamentoDto
    {
        [Required(ErrorMessage = "O identificador da Ordem de Serviço é obrigatório.")]
        public Guid IdOrdemServico { get; set; }

        public List<ItemOrcamentoDto> Itens { get; set; } = new();

        public string? Observacoes { get; set; }

        public int? ValidadeDias { get; set; } = 7;
    }

    public class OrcamentoResponseDto
    {
        public Guid Id { get; set; }
        public Guid IdOrdemServico { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAprovacao { get; set; }
        public DateTime? DataExpiracao { get; set; }
        public List<ItemOrcamentoDto> Itens { get; set; } = new();
    }
}
