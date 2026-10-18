using Fiap.TechChallenge.Billing.Domain.Enums;
using Fiap.TechChallenge.Billing.Domain.Exceptions;

namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public class Orcamento : EntidadeBase
    {
        public Guid IdOrdemServico { get; private set; }
        public List<ItemOrcamento> Itens { get; private set; } = new();
        public decimal ValorTotal { get; private set; }
        public StatusOrcamento Status { get; private set; }
        public string? Observacoes { get; private set; }
        public DateTime? DataAprovacao { get; private set; }
        public DateTime? DataCancelamento { get; private set; }
        public DateTime? DataExpiracao { get; private set; }

        public Orcamento() : base()
        {
            Status = StatusOrcamento.Pendente;
            Itens = new List<ItemOrcamento>();
        }

        public Orcamento(Guid idOrdemServico, IEnumerable<ItemOrcamento>? itens = null, string? observacoes = null, DateTime? dataExpiracao = null) : base()
        {
            if (idOrdemServico == Guid.Empty)
                throw new DomainException("O identificador da Ordem de Serviço é obrigatório.");

            IdOrdemServico = idOrdemServico;
            Status = StatusOrcamento.Pendente;
            Observacoes = observacoes;
            DataExpiracao = dataExpiracao ?? DateTime.UtcNow.AddDays(7);

            if (itens != null)
            {
                foreach (var item in itens)
                {
                    Itens.Add(item);
                }
            }

            RecalcularValorTotal();
        }

        public void AdicionarItem(ItemOrcamento item)
        {
            if (Status != StatusOrcamento.Pendente)
                throw new DomainException($"Não é possível alterar itens de um orçamento com status '{Status}'.");

            ArgumentNullException.ThrowIfNull(item);
            Itens.Add(item);
            RecalcularValorTotal();
            AtualizarTimestamp();
        }

        public void RemoverItem(Guid itemId)
        {
            if (Status != StatusOrcamento.Pendente)
                throw new DomainException($"Não é possível remover itens de um orçamento com status '{Status}'.");

            var item = Itens.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                Itens.Remove(item);
                RecalcularValorTotal();
                AtualizarTimestamp();
            }
        }

        public void Aprovar()
        {
            if (Status == StatusOrcamento.Aprovado)
                return;

            if (Status != StatusOrcamento.Pendente)
                throw new DomainException($"Não é possível aprovar um orçamento que está no status '{Status}'.");

            if (ValorTotal <= 0)
                throw new DomainException("Não é possível aprovar um orçamento com valor total zerado ou negativo.");

            Status = StatusOrcamento.Aprovado;
            DataAprovacao = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void Rejeitar(string? motivo = null)
        {
            if (Status != StatusOrcamento.Pendente)
                throw new DomainException($"Não é possível rejeitar um orçamento que está no status '{Status}'.");

            Status = StatusOrcamento.Rejeitado;
            if (!string.IsNullOrWhiteSpace(motivo))
                Observacoes = string.IsNullOrWhiteSpace(Observacoes) ? motivo : $"{Observacoes} | Motivo rejeição: {motivo}";

            AtualizarTimestamp();
        }

        public void Cancelar(string? motivo = null)
        {
            if (Status == StatusOrcamento.Cancelado)
                return;

            Status = StatusOrcamento.Cancelado;
            DataCancelamento = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(motivo))
                Observacoes = string.IsNullOrWhiteSpace(Observacoes) ? motivo : $"{Observacoes} | Motivo cancelamento: {motivo}";

            AtualizarTimestamp();
        }

        public void DefinirObservacoes(string? observacoes)
        {
            Observacoes = observacoes;
            AtualizarTimestamp();
        }

        private void RecalcularValorTotal()
        {
            ValorTotal = Itens.Sum(i => i.ValorTotal);
        }
    }
}
