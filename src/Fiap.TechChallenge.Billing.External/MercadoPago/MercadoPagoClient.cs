using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fiap.TechChallenge.Billing.Domain.Exceptions;
using Fiap.TechChallenge.Billing.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.Billing.External.MercadoPago
{
    public class MercadoPagoClient : IMercadoPagoService
    {
        private readonly HttpClient _httpClient;
        private readonly MercadoPagoSettings _settings;
        private readonly ILogger<MercadoPagoClient> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public MercadoPagoClient(
            HttpClient httpClient,
            IOptions<MercadoPagoSettings> settings,
            ILogger<MercadoPagoClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }

            if (_settings.TimeoutSeconds > 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            }
        }

        public async Task<CobrancaPixResult> GerarCobrancaPixAsync(CobrancaPixRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogInformation("Iniciando geração de cobrança PIX no Mercado Pago para OS '{ExternalRef}' no valor de {Valor:C}.",
                request.ExternalReference, request.Valor);

            var idempotencyKey = Guid.NewGuid().ToString("D");
            var notificationUrl = !string.IsNullOrWhiteSpace(request.NotificationUrl)
                ? request.NotificationUrl
                : _settings.NotificationUrl;

            var payload = new
            {
                transaction_amount = request.Valor,
                description = request.Descricao,
                payment_method_id = "pix",
                payer = new
                {
                    email = request.Email,
                    first_name = request.Nome,
                    identification = new
                    {
                        type = "CPF",
                        number = request.Cpf.Replace(".", "").Replace("-", "").Trim()
                    }
                },
                external_reference = request.ExternalReference,
                notification_url = string.IsNullOrWhiteSpace(notificationUrl) ? null : notificationUrl
            };

            var jsonContent = JsonSerializer.Serialize(payload, _jsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/payments");
            httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            httpRequest.Headers.Add("X-Idempotency-Key", idempotencyKey);

            if (!string.IsNullOrWhiteSpace(_settings.AccessToken))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            }

            try
            {
                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Falha na API Mercado Pago. Status: {StatusCode}. Resposta: {RawResponse}",
                        response.StatusCode, rawResponse);
                    throw new PaymentProcessingException($"Erro ao gerar cobrança no Mercado Pago: {response.StatusCode} - {rawResponse}");
                }

                using var doc = JsonDocument.Parse(rawResponse);
                var root = doc.RootElement;

                var id = root.GetProperty("id").ToString();
                var status = root.TryGetProperty("status", out var s) ? s.GetString() ?? "pending" : "pending";

                string? qrCode = null;
                string? qrCodeBase64 = null;
                string? ticketUrl = null;

                if (root.TryGetProperty("point_of_interaction", out var poi) &&
                    poi.TryGetProperty("transaction_data", out var td))
                {
                    if (td.TryGetProperty("qr_code", out var qc))
                        qrCode = qc.GetString();

                    if (td.TryGetProperty("qr_code_base64", out var qcb64))
                        qrCodeBase64 = qcb64.GetString();

                    if (td.TryGetProperty("ticket_url", out var tu))
                        ticketUrl = tu.GetString();
                }

                _logger.LogInformation("Cobrança PIX gerada com sucesso no Mercado Pago. ID: {PaymentId}, Status: {Status}.",
                    id, status);

                return new CobrancaPixResult(id, status, qrCode, qrCodeBase64, ticketUrl, rawResponse);
            }
            catch (Exception ex) when (ex is not PaymentProcessingException)
            {
                _logger.LogError(ex, "Exceção ao comunicar com o Mercado Pago para OS '{ExternalRef}'.", request.ExternalReference);
                throw new PaymentProcessingException($"Falha de comunicação com o Mercado Pago: {ex.Message}", ex);
            }
        }

        public async Task<ConsultaPagamentoResult> ConsultarPagamentoAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
                throw new ArgumentException("ID externo do pagamento é obrigatório.", nameof(externalPaymentId));

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/payments/{externalPaymentId}");
            if (!string.IsNullOrWhiteSpace(_settings.AccessToken))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            }

            try
            {
                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao consultar pagamento {PaymentId} no Mercado Pago. Status: {StatusCode}.",
                        externalPaymentId, response.StatusCode);
                    throw new PaymentProcessingException($"Erro ao consultar pagamento no Mercado Pago: {response.StatusCode}");
                }

                using var doc = JsonDocument.Parse(rawResponse);
                var root = doc.RootElement;

                var id = root.GetProperty("id").ToString();
                var status = root.TryGetProperty("status", out var s) ? s.GetString() ?? "unknown" : "unknown";
                var statusDetail = root.TryGetProperty("status_detail", out var sd) ? sd.GetString() : null;
                var amount = root.TryGetProperty("transaction_amount", out var ta) ? ta.GetDecimal() : 0m;
                var extRef = root.TryGetProperty("external_reference", out var er) ? er.GetString() : null;

                return new ConsultaPagamentoResult(id, status, statusDetail, amount, extRef, rawResponse);
            }
            catch (Exception ex) when (ex is not PaymentProcessingException)
            {
                _logger.LogError(ex, "Exceção ao consultar pagamento {PaymentId} no Mercado Pago.", externalPaymentId);
                throw new PaymentProcessingException($"Falha de comunicação ao consultar pagamento: {ex.Message}", ex);
            }
        }

        public async Task<EstornoResult> RealizarEstornoAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
                throw new ArgumentException("ID externo do pagamento é obrigatório.", nameof(externalPaymentId));

            _logger.LogInformation("Solicitando estorno para pagamento '{PaymentId}' no Mercado Pago.", externalPaymentId);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v1/payments/{externalPaymentId}/refunds");
            httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            if (!string.IsNullOrWhiteSpace(_settings.AccessToken))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            }

            try
            {
                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(rawResponse);
                    var status = doc.RootElement.TryGetProperty("status", out var s) ? s.GetString() ?? "approved" : "approved";
                    _logger.LogInformation("Estorno do pagamento '{PaymentId}' realizado com status '{Status}'.", externalPaymentId, status);
                    return new EstornoResult(true, externalPaymentId, status, "Estorno processado com sucesso.");
                }

                _logger.LogWarning("Não foi possível processar estorno de '{PaymentId}'. Status HTTP: {Code}, Detalhe: {Detail}",
                    externalPaymentId, response.StatusCode, rawResponse);
                return new EstornoResult(false, externalPaymentId, "failed", rawResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao tentar estorno de '{PaymentId}'.", externalPaymentId);
                return new EstornoResult(false, externalPaymentId, "error", ex.Message);
            }
        }

        public bool ValidarAssinaturaWebhook(string? xSignatureHeader, string? xRequestId, string requestBody)
        {
            // Se nenhum segredo configurado (ambiente local/testes), aceita para não travar desenvolvimento
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            {
                _logger.LogWarning("WebhookSecret não configurado. Validação HMAC do webhook ignorada em ambiente de desenvolvimento.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(xSignatureHeader))
            {
                _logger.LogWarning("Cabeçalho 'x-signature' ausente na requisição do Webhook Mercado Pago.");
                return false;
            }

            try
            {
                // Formato padrão do x-signature do Mercado Pago: ts=1700000000,v1=hash_hmac
                string? ts = null;
                string? hashRecebido = null;

                var parts = xSignatureHeader.Split(',');
                foreach (var part in parts)
                {
                    var kv = part.Split('=', 2);
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim();
                        var val = kv[1].Trim();
                        if (key.Equals("ts", StringComparison.OrdinalIgnoreCase)) ts = val;
                        if (key.Equals("v1", StringComparison.OrdinalIgnoreCase)) hashRecebido = val;
                    }
                }

                if (string.IsNullOrWhiteSpace(ts) || string.IsNullOrWhiteSpace(hashRecebido))
                {
                    _logger.LogWarning("Cabeçalho 'x-signature' com formato inválido: {Signature}", xSignatureHeader);
                    return false;
                }

                // Criação do manifesto de validação: "id:[data.id_url];request-id:[x-request-id];ts:[ts];"
                // ou concatenação com requestBody
                var manifest = $"ts:{ts};body:{requestBody};";
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
                var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
                var computedHash = Convert.ToHexString(computedHashBytes).ToLowerInvariant();

                var isValid = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computedHash),
                    Encoding.UTF8.GetBytes(hashRecebido.ToLowerInvariant()));

                if (!isValid)
                {
                    // Validação alternativa suportando data-id direto
                    var simpleManifest = $"ts:{ts};requestId:{xRequestId};";
                    var simpleHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(simpleManifest))).ToLowerInvariant();
                    isValid = CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(simpleHash),
                        Encoding.UTF8.GetBytes(hashRecebido.ToLowerInvariant()));
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular assinatura HMAC SHA-256 do webhook.");
                return false;
            }
        }
    }
}
