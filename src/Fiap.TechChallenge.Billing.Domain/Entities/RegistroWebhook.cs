namespace Fiap.TechChallenge.Billing.Domain.Entities
{
    public class RegistroWebhook
    {
        public Guid Id { get; private set; }
        public string? ExternalEventId { get; private set; }
        public string? Action { get; private set; }
        public string? StatusRecebido { get; private set; }
        public bool AssinaturaValida { get; private set; }
        public string PayloadJson { get; private set; }
        public DateTime DataRecebimento { get; private set; }
        public string? MensagemProcessamento { get; private set; }

        public RegistroWebhook()
        {
            Id = Guid.NewGuid();
            PayloadJson = string.Empty;
            DataRecebimento = DateTime.UtcNow;
        }

        public RegistroWebhook(
            string? externalEventId,
            string? action,
            string? statusRecebido,
            bool assinaturaValida,
            string payloadJson,
            string? mensagemProcessamento = null)
        {
            Id = Guid.NewGuid();
            ExternalEventId = externalEventId;
            Action = action;
            StatusRecebido = statusRecebido;
            AssinaturaValida = assinaturaValida;
            PayloadJson = payloadJson ?? string.Empty;
            DataRecebimento = DateTime.UtcNow;
            MensagemProcessamento = mensagemProcessamento;
        }
    }
}
