using System.ComponentModel.DataAnnotations;

namespace Fiap.TechChallenge.Billing.Application.DTOs.Pagamento
{
    public class DadosPagadorDto
    {
        [Required(ErrorMessage = "O CPF do pagador é obrigatório.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do pagador é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail do pagador é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public class CriarPagamentoDto
    {
        [Required(ErrorMessage = "O identificador da Ordem de Serviço é obrigatório.")]
        public Guid IdOrdemServico { get; set; }

        [Required(ErrorMessage = "O identificador do Orçamento é obrigatório.")]
        public Guid IdOrcamento { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Os dados do pagador são obrigatórios.")]
        public DadosPagadorDto Pagador { get; set; } = null!;
    }

    public class PagamentoResponseDto
    {
        public Guid Id { get; set; }
        public Guid IdOrdemServico { get; set; }
        public Guid IdOrcamento { get; set; }
        public string? ExternalPaymentId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
        public string? QrCode { get; set; }
        public string? QrCodeBase64 { get; set; }
        public DadosPagadorDto Pagador { get; set; } = null!;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string? MotivoFalha { get; set; }
    }

    public class EstornoPagamentoDto
    {
        [Required(ErrorMessage = "O motivo do estorno é obrigatório.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
