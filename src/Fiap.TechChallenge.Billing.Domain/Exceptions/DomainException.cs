namespace Fiap.TechChallenge.Billing.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, object id)
            : base($"{entityName} com identificador '{id}' não foi encontrado(a).") { }
    }

    public class PaymentProcessingException : DomainException
    {
        public PaymentProcessingException(string message) : base(message) { }
        public PaymentProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
