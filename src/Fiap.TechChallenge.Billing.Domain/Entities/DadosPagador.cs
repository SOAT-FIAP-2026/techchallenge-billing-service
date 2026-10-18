using Fiap.TechChallenge.Billing.Domain.Exceptions;

namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public class DadosPagador
    {
        public string Cpf { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }

        public DadosPagador()
        {
            Cpf = string.Empty;
            Nome = string.Empty;
            Email = string.Empty;
        }

        public DadosPagador(string cpf, string nome, string email)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new DomainException("CPF do pagador é obrigatório.");

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome do pagador é obrigatório.");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("E-mail do pagador é obrigatório.");

            Cpf = cpf.Replace(".", "").Replace("-", "").Trim();
            Nome = nome.Trim();
            Email = email.Trim().ToLowerInvariant();
        }
    }
}
