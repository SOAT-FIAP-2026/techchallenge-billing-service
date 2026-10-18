using Fiap.TechChallenge.Billing.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fiap.TechChallenge.Billing.Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MongoDbSettings> options, ILogger<MongoDbContext> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            BsonClassMappings.RegisterMappings();

            var mongoUrl = new MongoUrl(_settings.ConnectionString);
            var clientSettings = MongoClientSettings.FromUrl(mongoUrl);
            _client = new MongoClient(clientSettings);

            var dbName = !string.IsNullOrWhiteSpace(_settings.DatabaseName)
                ? _settings.DatabaseName
                : (mongoUrl.DatabaseName ?? "techchallenge_billing_db");

            _database = _client.GetDatabase(dbName);

            _logger.LogInformation("MongoDbContext inicializado com sucesso para o banco de dados '{DatabaseName}'.", dbName);
        }

        public IMongoClient Client => _client;

        public IMongoDatabase Database => _database;

        public IMongoCollection<Orcamento> Orcamentos =>
            _database.GetCollection<Orcamento>(_settings.OrcamentosCollectionName);

        public IMongoCollection<Pagamento> Pagamentos =>
            _database.GetCollection<Pagamento>(_settings.PagamentosCollectionName);

        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new BsonDocument("ping", 1);
                await _database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao executar ping no MongoDB.");
                return false;
            }
        }

        public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Índices para Orcamentos
                var orcamentoIndexBuilder = Builders<Orcamento>.IndexKeys;
                var orcamentoIndexModel = new CreateIndexModel<Orcamento>(
                    orcamentoIndexBuilder.Ascending(o => o.IdOrdemServico),
                    new CreateIndexOptions { Name = "idx_orcamento_id_ordem_servico" }
                );
                await Orcamentos.Indexes.CreateOneAsync(orcamentoIndexModel, cancellationToken: cancellationToken);

                // Índices para Pagamentos
                var pagamentoIndexBuilder = Builders<Pagamento>.IndexKeys;
                var pagamentoOsIndexModel = new CreateIndexModel<Pagamento>(
                    pagamentoIndexBuilder.Ascending(p => p.IdOrdemServico),
                    new CreateIndexOptions { Name = "idx_pagamento_id_ordem_servico" }
                );
                var pagamentoExtIndexModel = new CreateIndexModel<Pagamento>(
                    pagamentoIndexBuilder.Ascending(p => p.ExternalPaymentId),
                    new CreateIndexOptions { Name = "idx_pagamento_external_id", Sparse = true }
                );

                await Pagamentos.Indexes.CreateManyAsync(
                    new[] { pagamentoOsIndexModel, pagamentoExtIndexModel },
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Índices das coleções MongoDB verificados/criados com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível criar índices no MongoDB durante a inicialização (pode ser executado com banco offline).");
            }
        }
    }
}
