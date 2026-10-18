using Fiap.TechChallenge.Billing.Domain.Enums;
using Fiap.TechChallenge.Billing.Domain.Exceptions;

namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public class ItemOrcamento
    {
        public Guid Id { get; private set; }
        public Guid? ItemCatalogoId { get; private set; }
        public string Descricao { get; private set; }
        public TipoItemOrcamento Tipo { get; private set; }
        public int Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal => Quantidade * ValorUnitario;

        // Construtor para deserialização
        public ItemOrcamento() 
        {
            Id = Guid.NewGuid();
            Descricao = string.Empty;
        }

        public ItemOrcamento(string descricao, TipoItemOrcamento tipo, int quantidade, decimal valorUnitario, Guid? itemCatalogoId = null)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição do item de orçamento é obrigatória.");

            if (quantidade <= 0)
                throw new DomainException("A quantidade do item deve ser maior que zero.");

            if (valorUnitario < 0)
                throw new DomainException("O valor unitário do item não pode ser negativo.");

            Id = Guid.NewGuid();
            Descricao = descricao.Trim();
            Tipo = tipo;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ItemCatalogoId = itemCatalogoId;
        }
    }
}
