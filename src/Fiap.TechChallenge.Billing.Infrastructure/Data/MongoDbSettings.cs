namespace Fiap.TechChallenge.Billing.Infrastructure.Data
{
    public class MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";

        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "techchallenge_billing_db";
        public string OrcamentosCollectionName { get; set; } = "orcamentos";
        public string PagamentosCollectionName { get; set; } = "pagamentos";
    }
}
