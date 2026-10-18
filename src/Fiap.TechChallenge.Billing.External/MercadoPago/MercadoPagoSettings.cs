namespace Fiap.TechChallenge.Billing.External.MercadoPago
{
    public class MercadoPagoSettings
    {
        public const string SectionName = "MercadoPagoSettings";

        public string BaseUrl { get; set; } = "https://api.mercadopago.com";
        public string AccessToken { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string NotificationUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
