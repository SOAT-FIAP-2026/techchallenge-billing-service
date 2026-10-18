namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public abstract class EntidadeBase
    {
        public Guid Id { get; protected set; }
        public DateTime DataCriacao { get; protected set; }
        public DateTime? DataAtualizacao { get; protected set; }
        public bool Ativo { get; protected set; }

        protected EntidadeBase()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
        }

        protected EntidadeBase(Guid id)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
        }

        public virtual void AtualizarTimestamp()
        {
            DataAtualizacao = DateTime.UtcNow;
        }

        public virtual void Desativar()
        {
            Ativo = false;
            AtualizarTimestamp();
        }

        public virtual void Ativar()
        {
            Ativo = true;
            AtualizarTimestamp();
        }
    }
}
