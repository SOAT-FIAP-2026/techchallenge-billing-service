using Fiap.TechChallenge.Billing.Domain.Enums;
using Fiap.TechChallenge.Billing.Domain.Exceptions;

namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public class Pagamento : EntidadeBase
    {
        public Guid IdOrdemServico { get; private set; }
        public Guid IdOrcamento { get; private set; }
        public string? ExternalPaymentId { get; private set; }
        public decimal Valor { get; private set; }
        public StatusPagamento Status { get; private set; }
        public MetodoPagamento Metodo { get; private set; }
        public string? QrCode { get; private set; }
        public string? QrCodeBase64 { get; private set; }
        public DadosPagador Pagador { get; private set; } = null!;
        public List<RegistroWebhook> HistoricoWebhooks { get; private set; } = new();
        public DateTime? DataConfirmacao { get; private set; }
        public DateTime? DataFalha { get; private set; }
        public DateTime? DataEstorno { get; private set; }
        public string? MotivoFalha { get; private set; }

        public Pagamento() : base()
        {
            Status = StatusPagamento.Pendente;
            Metodo = MetodoPagamento.Pix;
            HistoricoWebhooks = new List<RegistroWebhook>();
        }

        public Pagamento(Guid idOrdemServico, Guid idOrcamento, decimal valor, DadosPagador pagador, MetodoPagamento metodo = MetodoPagamento.Pix) : base()
        {
            if (idOrdemServico == Guid.Empty)
                throw new DomainException("O identificador da Ordem de Serviço é obrigatório.");

            if (idOrcamento == Guid.Empty)
                throw new DomainException("O identificador do Orçamento é obrigatório.");

            if (valor <= 0)
                throw new DomainException("O valor do pagamento deve ser estritamente maior que zero.");

            ArgumentNullException.ThrowIfNull(pagador);

            IdOrdemServico = idOrdemServico;
            IdOrcamento = idOrcamento;
            Valor = valor;
            Pagador = pagador;
            Metodo = metodo;
            Status = StatusPagamento.Pendente;
        }

        public void RegistrarCobrancaGerada(string externalPaymentId, string qrCode, string qrCodeBase64)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
                throw new DomainException("O ID externo da transação do gateway é obrigatório.");

            ExternalPaymentId = externalPaymentId;
            QrCode = qrCode;
            QrCodeBase64 = qrCodeBase64;
            Status = StatusPagamento.AguardandoPagamento;
            AtualizarTimestamp();
        }

        public void ConfirmarPagamento(string? externalPaymentId = null)
        {
            if (Status == StatusPagamento.Aprovado)
                return; // Idempotência

            if (Status == StatusPagamento.Estornado || Status == StatusPagamento.Cancelado)
                throw new DomainException($"Não é possível confirmar um pagamento com status '{Status}'.");

            if (!string.IsNullOrWhiteSpace(externalPaymentId))
                ExternalPaymentId = externalPaymentId;

            Status = StatusPagamento.Aprovado;
            DataConfirmacao = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void FalharPagamento(string motivo)
        {
            if (Status == StatusPagamento.Aprovado)
                throw new DomainException("Não é possível marcar como falhado um pagamento já aprovado.");

            Status = StatusPagamento.Falhado;
            MotivoFalha = motivo;
            DataFalha = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void Cancelar(string motivo)
        {
            if (Status == StatusPagamento.Aprovado)
                throw new DomainException("Pagamentos aprovados não podem ser cancelados diretamente, utilize estorno.");

            Status = StatusPagamento.Cancelado;
            MotivoFalha = motivo;
            DataFalha = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void Estornar(string motivo)
        {
            if (Status != StatusPagamento.Aprovado)
                throw new DomainException($"Apenas pagamentos aprovados podem ser estornados. Status atual: '{Status}'.");

            Status = StatusPagamento.Estornado;
            MotivoFalha = $"Estornado: {motivo}";
            DataEstorno = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void AdicionarRegistroWebhook(RegistroWebhook webhook)
        {
            ArgumentNullException.ThrowIfNull(webhook);
            HistoricoWebhooks.Add(webhook);
            AtualizarTimestamp();
        }
    }
}
