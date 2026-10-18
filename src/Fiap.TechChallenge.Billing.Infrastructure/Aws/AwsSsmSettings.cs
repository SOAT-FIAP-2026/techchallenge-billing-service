namespace Fiap.TechChallenge.Billing.Infrastructure.Aws
{
    public class AwsSsmSettings
    {
        public const string SectionName = "AwsSsmSettings";

        public bool Enabled { get; set; } = false;
        public string Region { get; set; } = "sa-east-1";
        public string BasePath { get; set; } = "/techchallenge/prod/";

        // Parâmetros padronizados da arquitetura
        public string MongoDbConnectionStringParam { get; set; } = "/techchallenge/prod/mongodb_connection_string";
        public string MercadoPagoAccessTokenParam { get; set; } = "/techchallenge/prod/mercadopago_access_token";
        public string MercadoPagoWebhookSecretParam { get; set; } = "/techchallenge/prod/mercadopago_webhook_secret";
        public string RabbitMqConnectionStringParam { get; set; } = "/techchallenge/prod/rabbitmq_connection_string";
        public string JwtSecretParam { get; set; } = "/techchallenge/prod/jwt_secret";
    }
}
