using System.Diagnostics.Metrics;

namespace Fiap.TechChallenge.Billing.Api.Observability
{
    public sealed class BillingObservabilityMetrics
    {
        public const string MeterName = "Fiap.TechChallenge.Billing.Observability";

        private readonly Counter<long> _orcamentosCreated;
        private readonly Counter<long> _paymentsCreated;
        private readonly Counter<long> _paymentsApproved;
        private readonly Counter<long> _paymentsFailed;
        private readonly Counter<long> _paymentsRefunded;
        private readonly Histogram<double> _httpRequestDuration;

        public BillingObservabilityMetrics()
        {
            var meter = new Meter(MeterName, "1.0.0");

            _orcamentosCreated = meter.CreateCounter<long>(
                "billing.orcamentos.created",
                unit: "{orcamento}",
                description: "Quantidade de orçamentos gerados.");

            _paymentsCreated = meter.CreateCounter<long>(
                "billing.payments.created",
                unit: "{payment}",
                description: "Quantidade de transações de cobrança iniciadas.");

            _paymentsApproved = meter.CreateCounter<long>(
                "billing.payments.approved",
                unit: "{payment}",
                description: "Quantidade de pagamentos confirmados com sucesso.");

            _paymentsFailed = meter.CreateCounter<long>(
                "billing.payments.failed",
                unit: "{payment}",
                description: "Quantidade de pagamentos que falharam ou foram cancelados.");

            _paymentsRefunded = meter.CreateCounter<long>(
                "billing.payments.refunded",
                unit: "{payment}",
                description: "Quantidade de pagamentos estornados.");

            _httpRequestDuration = meter.CreateHistogram<double>(
                "billing.http.request.duration",
                unit: "ms",
                description: "Latência das requisições HTTP da API de Billing.");
        }

        public void RecordOrcamentoCreated(string status) =>
            _orcamentosCreated.Add(1, new KeyValuePair<string, object?>("status", status));

        public void RecordPaymentCreated(string metodo) =>
            _paymentsCreated.Add(1, new KeyValuePair<string, object?>("metodo", metodo));

        public void RecordPaymentApproved() =>
            _paymentsApproved.Add(1);

        public void RecordPaymentFailed(string motivo) =>
            _paymentsFailed.Add(1, new KeyValuePair<string, object?>("motivo", motivo));

        public void RecordPaymentRefunded() =>
            _paymentsRefunded.Add(1);

        public void RecordHttpRequest(TimeSpan duration, string method, int statusCode) =>
            _httpRequestDuration.Record(
                Math.Max(0, duration.TotalMilliseconds),
                new KeyValuePair<string, object?>("method", method),
                new KeyValuePair<string, object?>("status_code", statusCode));
    }
}
